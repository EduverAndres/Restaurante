import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

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

export interface Restaurant {
  id: string;
  name: string;
  slug: string;
  description: string;
  logoUrl?: string;
  coverImageUrl?: string;
  themeConfig: ThemeConfig;
  isActive: boolean;
  ownerId: string;
  createdAt: string;
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

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private readonly apiUrl = `${environment.apiUrl}/restaurants`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Restaurant[]> {
    return this.http.get<any>(`${this.apiUrl}`).pipe(
      map((res: any) => res.data || res)
    );
  }

  getBySlug(slug: string): Observable<Restaurant> {
    return this.http.get<any>(`${this.apiUrl}/slug/${slug}`).pipe(
      map((res: any) => res.data || res)
    );
  }

  getByOwner(): Observable<Restaurant[]> {
    return this.http.get<any>(`${this.apiUrl}/owner`).pipe(
      map((res: any) => res.data || res)
    );
  }

  getById(id: string): Observable<Restaurant> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map((res: any) => res.data || res)
    );
  }

  create(data: Partial<Restaurant>): Observable<Restaurant> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => res.data || res)
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
}