import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiErrorEnvelope } from './api-response';
import { Order, normalizeOrder } from './order.service';

export interface ThemeConfig {
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  backgroundColor: string;
  textColor: string;
  fontFamily: string;
  logoUrl?: string;
  coverImageUrl?: string;
}

export interface BusinessHour {
  dayOfWeek: number; // 0 = Sunday
  openTime: string; // "HH:mm:ss"
  closeTime: string;
  isClosed: boolean;
}

export interface Restaurant {
  id: string;
  name: string;
  slug: string;
  description: string;
  /** Backend field: `logo` (RestaurantDto). Legacy alias `logoUrl` kept for restaurant pages (Fase 4). */
  logo?: string;
  /** Backend field: `coverImage` (RestaurantDto). Legacy alias `coverImageUrl` kept for restaurant pages (Fase 4). */
  coverImage?: string;
  /** @deprecated Legacy alias — the backend returns `logo`. */
  logoUrl?: string;
  /** @deprecated Legacy alias — the backend returns `coverImage`. */
  coverImageUrl?: string;
  themeConfig: ThemeConfig;
  isActive: boolean;
  ownerId: string;
  createdAt: string;
  phone?: string;
  latitude?: number;
  longitude?: number;
  radiusKm?: number;
  deliveryFee?: number;
  minOrderAmount?: number;
  estimatedPrepTimeMinutes?: number;
  averageRating?: number;
  reviewCount?: number;
  businessHours?: BusinessHour[];
}

export interface MenuCategory {
  id: string;
  restaurantId: string;
  name: string;
  description?: string;
  displayOrder: number;
  items: MenuItem[];
}

export interface MenuItem {
  id: string;
  restaurantId: string;
  categoryId: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  images?: string[];
  isAvailable: boolean;
  displayOrder: number;
  preparationTime?: number;
}

export interface TopProduct {
  menuItemId: string;
  name: string;
  quantity: number;
  revenue: number;
}

/** DTO for GET /api/restaurants/{id}/dashboard (GetRestaurantDashboardQuery). */
export interface RestaurantDashboard {
  salesToday: number;
  salesThisWeek: number;
  salesThisMonth: number;
  orderCountsByStatus: Record<string, number>;
  topProducts: TopProduct[];
  averagePrepTimeMinutes: number | null;
  recentOrders: Order[];
  totalOrders: number;
  totalRevenue: number;
}

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private readonly apiUrl = `${environment.apiUrl}/restaurants`;

  constructor(private http: HttpClient) {}

  /**
   * Backend RestaurantDto exposes `logo`/`coverImage`; older frontend code reads
   * `logoUrl`/`coverImageUrl`. Normalize once here so both shapes work and the
   * canonical fields (`logo`/`coverImage`) always carry the value.
   */
  private normalizeRestaurant(dto: any): Restaurant {
    return {
      ...dto,
      logo: dto.logo ?? dto.logoUrl,
      coverImage: dto.coverImage ?? dto.coverImageUrl,
      // Backfill legacy aliases so pre-Fase-4 pages keep working unchanged.
      logoUrl: dto.logoUrl ?? dto.logo,
      coverImageUrl: dto.coverImageUrl ?? dto.coverImage,
    };
  }

  /**
   * Business validation failures arrive as HTTP 200 with { success: false, message, data: null }
   * (see apiResponseInterceptor). Surface them as thrown errors so callers can read `message`.
   */
  private unwrap<T>(value: any): T {
    if (isApiErrorEnvelope(value)) throw value;
    return value as T;
  }

  getAll(): Observable<Restaurant[]> {
    return this.http.get<any>(`${this.apiUrl}`).pipe(
      map((res: any) => {
        const list = res.data || res;
        return Array.isArray(list) ? list.map(r => this.normalizeRestaurant(r)) : list;
      })
    );
  }

  getBySlug(slug: string): Observable<Restaurant> {
    return this.http.get<any>(`${this.apiUrl}/slug/${slug}`).pipe(
      map((res: any) => {
        const restaurant = res.data ?? res;
        if (!restaurant || !restaurant.id) {
          throw new Error('Restaurant not found or invalid response.');
        }
        return this.normalizeRestaurant(restaurant);
      })
    );
  }

  getByOwner(): Observable<Restaurant[]> {
    return this.http.get<any>(`${this.apiUrl}/owner`).pipe(
      map((res: any) => {
        const list = res.data || res;
        return Array.isArray(list) ? list.map(r => this.normalizeRestaurant(r)) : list;
      })
    );
  }

  getById(id: string): Observable<Restaurant> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map((res: any) => this.normalizeRestaurant(res.data || res))
    );
  }

  create(data: Partial<Restaurant>): Observable<Restaurant> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => this.unwrap<Restaurant>(res.data || res))
    );
  }

  update(id: string, data: Partial<Restaurant>): Observable<Restaurant> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  getMenu(restaurantId: string): Observable<MenuCategory[]> {
    return this.http.get<any>(`${environment.apiUrl}/restaurants/${restaurantId}/menu`).pipe(
      map((res: any) => res.data || res)
    );
  }

  createMenuItem(restaurantId: string, data: Partial<MenuItem>): Observable<MenuItem> {
    return this.http.post<any>(`${environment.apiUrl}/restaurants/${restaurantId}/menu`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  updateMenuItem(restaurantId: string, itemId: string, data: Partial<MenuItem>): Observable<MenuItem> {
    return this.http.put<any>(`${environment.apiUrl}/restaurants/${restaurantId}/menu/${itemId}`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  deleteMenuItem(restaurantId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/restaurants/${restaurantId}/menu/${itemId}`);
  }

  createCategory(restaurantId: string, data: Partial<MenuCategory>): Observable<MenuCategory> {
    return this.http.post<any>(`${environment.apiUrl}/restaurants/${restaurantId}/categories`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  updateCategory(restaurantId: string, categoryId: string, data: Partial<MenuCategory>): Observable<MenuCategory> {
    return this.http.put<any>(`${environment.apiUrl}/restaurants/${restaurantId}/categories/${categoryId}`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  deleteCategory(restaurantId: string, categoryId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/restaurants/${restaurantId}/categories/${categoryId}`);
  }

  /** PUT /api/restaurants/{id}/business-hours — body is `{ hours: BusinessHour[] }` (full replace, max 7 days). */
  updateBusinessHours(id: string, hours: BusinessHour[]): Observable<Restaurant> {
    return this.http.put<any>(`${this.apiUrl}/${id}/business-hours`, { hours }).pipe(
      map((res: any) => this.normalizeRestaurant(this.unwrap(res.data || res)))
    );
  }

  /** PUT /api/restaurants/{id}/delivery-settings — deliveryFee, minOrderAmount, radiusKm?, estimatedPrepTimeMinutes?. */
  updateDeliverySettings(id: string, settings: {
    deliveryFee: number;
    minOrderAmount: number;
    radiusKm?: number | null;
    estimatedPrepTimeMinutes?: number | null;
  }): Observable<Restaurant> {
    return this.http.put<any>(`${this.apiUrl}/${id}/delivery-settings`, settings).pipe(
      map((res: any) => this.normalizeRestaurant(this.unwrap(res.data || res)))
    );
  }

  /** POST /api/restaurants/{id}/images — multipart form: field `type` = 'logo' | 'cover', field `file` = the image. */
  uploadImage(id: string, type: 'logo' | 'cover', file: File): Observable<Restaurant> {
    const form = new FormData();
    form.append('type', type);
    form.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/${id}/images`, form).pipe(
      map((res: any) => this.normalizeRestaurant(this.unwrap(res.data || res)))
    );
  }

  /** PATCH /api/restaurants/{restaurantId}/menu/{itemId}/availability — body is `{ isAvailable }`. */
  updateItemAvailability(restaurantId: string, itemId: string, isAvailable: boolean): Observable<MenuItem> {
    return this.http.patch<any>(`${environment.apiUrl}/restaurants/${restaurantId}/menu/${itemId}/availability`, { isAvailable }).pipe(
      map((res: any) => this.unwrap<MenuItem>(res.data || res))
    );
  }

  /** GET /api/restaurants/{id}/dashboard — metrics for the owner's dashboard (RestaurantOwner). */
  getDashboard(restaurantId: string): Observable<RestaurantDashboard> {
    return this.http.get<any>(`${this.apiUrl}/${restaurantId}/dashboard`).pipe(
      map((res: any) => {
        const data = res.data || res;
        return {
          ...data,
          recentOrders: Array.isArray(data.recentOrders) ? (data.recentOrders as any[]).map((o: any) => normalizeOrder(o)) : [],
        } as RestaurantDashboard;
      })
    );
  }
}