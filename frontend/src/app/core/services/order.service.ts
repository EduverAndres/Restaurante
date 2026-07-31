import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderItem {
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
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
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  restaurantId: string;
  items: { menuItemId: string; quantity: number }[];
  customerNote?: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  getCustomerOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/customer`);
  }

  getRestaurantOrders(restaurantId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/restaurant/${restaurantId}`);
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`);
  }

  createOrder(data: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.apiUrl, data);
  }

  updateStatus(orderId: string, status: string): Observable<Order> {
    return this.http.patch<Order>(`${this.apiUrl}/${orderId}/status`, { status });
  }
}
