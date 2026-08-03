import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiErrorEnvelope } from './api-response';

export interface CustomerAddress {
  id: string;
  label: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  isDefault: boolean;
}

export interface CreateAddressRequest {
  label: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  isDefault?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AddressService {
  private readonly apiUrl = `${environment.apiUrl}/customer/addresses`;

  constructor(private http: HttpClient) {}

  /**
   * Business validation failures arrive as HTTP 200 with { success: false, message, data: null }
   * (see apiResponseInterceptor). Surface them as thrown errors so callers can read `message`.
   */
  private unwrap<T>(value: any): T {
    if (isApiErrorEnvelope(value)) throw value;
    return value as T;
  }

  getAddresses(): Observable<CustomerAddress[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map((res: any) => this.unwrap<CustomerAddress[]>(res.data || res))
    );
  }

  createAddress(data: CreateAddressRequest): Observable<CustomerAddress> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => this.unwrap<CustomerAddress>(res.data || res))
    );
  }

  updateAddress(id: string, data: CreateAddressRequest): Observable<CustomerAddress> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data).pipe(
      map((res: any) => this.unwrap<CustomerAddress>(res.data || res))
    );
  }

  deleteAddress(id: string): Observable<boolean> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(
      map((res: any) => this.unwrap<boolean>(res.data !== undefined ? res.data : res))
    );
  }
}
