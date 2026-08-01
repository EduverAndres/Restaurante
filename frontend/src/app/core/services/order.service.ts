import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderItem {
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
}

export interface Order {
  id: string;
  customerId: string;
  restaurantId: string;
  restaurantName: string;
  items: OrderItem[];
  total: number;
  status: 'pending' | 'confirmed' | 'preparing' | 'ready' | 'delivered' | 'cancelled';
  customerNote?: string;
  notes?: string;
  paymentStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  restaurantId: string;
  items: { menuItemId: string; quantity: number; notes?: string }[];
  customerNote?: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  getCustomerOrders(): Observable<Order[]> {
    return this.http.get<any>(`${this.apiUrl}/customer`).pipe(
      map((res: any) => res.data || res)
    );
  }

  getRestaurantOrders(restaurantId: string): Observable<Order[]> {
    return this.http.get<any>(`${this.apiUrl}/restaurant/${restaurantId}`).pipe(
      map((res: any) => res.data || res)
    );
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<any>(`${this.apiUrl}/${orderId}`).pipe(
      map((res: any) => res.data || res)
    );
  }

  createOrder(data: CreateOrderRequest): Observable<Order> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  updateStatus(orderId: string, status: string): Observable<Order> {
    return this.http.put<any>(`${this.apiUrl}/${orderId}/status`, { status }).pipe(
      map((res: any) => res.data || res)
    );
  }
}