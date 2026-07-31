import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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
  isAvailable: boolean;
  displayOrder: number;
}

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private readonly apiUrl = `${environment.apiUrl}/restaurants`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(this.apiUrl);
  }

  getBySlug(slug: string): Observable<Restaurant> {
    return this.http.get<Restaurant>(`${this.apiUrl}/slug/${slug}`);
  }

  getByOwner(): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(`${this.apiUrl}/owner`);
  }

  getById(id: string): Observable<Restaurant> {
    return this.http.get<Restaurant>(`${this.apiUrl}/${id}`);
  }

  create(data: Partial<Restaurant>): Observable<Restaurant> {
    return this.http.post<Restaurant>(this.apiUrl, data);
  }

  update(id: string, data: Partial<Restaurant>): Observable<Restaurant> {
    return this.http.put<Restaurant>(`${this.apiUrl}/${id}`, data);
  }

  getMenu(restaurantId: string): Observable<MenuCategory[]> {
    return this.http.get<MenuCategory[]>(`${this.apiUrl}/${restaurantId}/menu`);
  }

  createMenuItem(restaurantId: string, categoryId: string, data: Partial<MenuItem>): Observable<MenuItem> {
    return this.http.post<MenuItem>(`${this.apiUrl}/${restaurantId}/menu/${categoryId}`, data);
  }

  updateMenuItem(restaurantId: string, itemId: string, data: Partial<MenuItem>): Observable<MenuItem> {
    return this.http.put<MenuItem>(`${this.apiUrl}/${restaurantId}/menu/${itemId}`, data);
  }

  deleteMenuItem(restaurantId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${restaurantId}/menu/${itemId}`);
  }

  createCategory(restaurantId: string, data: Partial<MenuCategory>): Observable<MenuCategory> {
    return this.http.post<MenuCategory>(`${this.apiUrl}/${restaurantId}/categories`, data);
  }

  updateCategory(restaurantId: string, categoryId: string, data: Partial<MenuCategory>): Observable<MenuCategory> {
    return this.http.put<MenuCategory>(`${this.apiUrl}/${restaurantId}/categories/${categoryId}`, data);
  }

  deleteCategory(restaurantId: string, categoryId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${restaurantId}/categories/${categoryId}`);
  }
}
