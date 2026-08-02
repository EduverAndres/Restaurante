import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiErrorEnvelope } from './api-response';

export type CouponDiscountType = 'Percentage' | 'Fixed';

export interface Coupon {
  id: string;
  code: string;
  discountType: CouponDiscountType;
  discountValue: number;
  restaurantId?: string | null;
  validFrom: string;
  validUntil: string;
  maxUses?: number | null;
  timesUsed: number;
  minOrderAmount: number;
  isActive: boolean;
}

/** Body for POST /api/restaurants/{restaurantId}/coupons (code, discountType, discountValue, validFrom, validUntil, maxUses?, minOrderAmount?). */
export interface CreateCouponPayload {
  code: string;
  discountType: CouponDiscountType;
  discountValue: number;
  validFrom: string;
  validUntil: string;
  maxUses?: number | null;
  minOrderAmount?: number | null;
}

/** Body for PUT /api/restaurants/{restaurantId}/coupons/{couponId} (code/discountType are NOT updatable). */
export interface UpdateCouponPayload {
  discountValue: number;
  validFrom: string;
  validUntil: string;
  maxUses?: number | null;
  minOrderAmount?: number | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private readonly apiUrl = `${environment.apiUrl}/restaurants`;

  constructor(private http: HttpClient) {}

  /**
   * Business validation failures arrive as HTTP 200 with { success: false, message, data: null }
   * (see apiResponseInterceptor). Surface them as thrown errors so callers can read `message`.
   */
  private unwrap<T>(value: any): T {
    if (isApiErrorEnvelope(value)) throw value;
    return value as T;
  }

  getRestaurantCoupons(restaurantId: string): Observable<Coupon[]> {
    return this.http.get<any>(`${this.apiUrl}/${restaurantId}/coupons`).pipe(
      map((res: any) => {
        const list = res.data || res;
        return Array.isArray(list) ? (list as Coupon[]) : [];
      })
    );
  }

  createCoupon(restaurantId: string, data: CreateCouponPayload): Observable<Coupon> {
    return this.http.post<any>(`${this.apiUrl}/${restaurantId}/coupons`, data).pipe(
      map((res: any) => this.unwrap<Coupon>(res.data || res))
    );
  }

  updateCoupon(restaurantId: string, couponId: string, data: UpdateCouponPayload): Observable<Coupon> {
    return this.http.put<any>(`${this.apiUrl}/${restaurantId}/coupons/${couponId}`, data).pipe(
      map((res: any) => this.unwrap<Coupon>(res.data || res))
    );
  }

  deleteCoupon(restaurantId: string, couponId: string): Observable<boolean> {
    return this.http.delete<any>(`${this.apiUrl}/${restaurantId}/coupons/${couponId}`).pipe(
      map((res: any) => this.unwrap<boolean>(res.data ?? true))
    );
  }
}
