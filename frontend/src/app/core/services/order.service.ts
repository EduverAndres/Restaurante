import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiErrorEnvelope, isApiErrorEnvelope } from './api-response';

export interface OrderItem {
  menuItemId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
}

export type OrderStatus =
  | 'pending'
  | 'confirmed'
  | 'preparing'
  | 'ready'
  | 'assignedToRider'
  | 'outForDelivery'
  | 'delivered'
  | 'cancelled';

export interface Order {
  id: string;
  customerId: string;
  restaurantId: string;
  restaurantName: string;
  customerName: string;
  items: OrderItem[];
  total: number;
  deliveryFee: number;
  discountAmount: number;
  couponId?: string | null;
  status: OrderStatus;
  customerNote?: string;
  notes?: string;
  paymentStatus?: string;
  /** Backend PaymentMethod ("CASH" | "CARD"); absent before payment. */
  paymentMethod?: string | null;
  deliveryAddress?: string | null;
  /** Destination coordinates (not exposed by the backend OrderDto; filled
   *  client-side from the customer's default address when available). */
  latitude?: number | null;
  longitude?: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  restaurantId: string;
  items: { menuItemId: string; quantity: number; notes?: string }[];
  customerNote?: string;
  notes?: string;
  deliveryAddress?: string;
  latitude?: number | null;
  longitude?: number | null;
}

const STATUS_NORMALIZATION: Record<string, OrderStatus> = {
  Pending: 'pending',
  Confirmed: 'confirmed',
  Preparing: 'preparing',
  Ready: 'ready',
  AssignedToRider: 'assignedToRider',
  OutForDelivery: 'outForDelivery',
  Delivered: 'delivered',
  Cancelled: 'cancelled',
};

const PAYMENT_STATUS_NORMALIZATION: Record<string, string> = {
  Pending: 'pending',
  Paid: 'paid',
  Failed: 'failed',
  Refunded: 'refunded',
};

/**
 * The backend serializes PaymentStatus enums as PascalCase ("Paid"); the
 * frontend uses lowercase values. Null-safe: missing statuses yield undefined
 * so components can rely on `paymentStatus` being a string or undefined.
 */
export function normalizePaymentStatus(status: string | null | undefined): string | undefined {
  if (status == null || status === '') return undefined;
  return PAYMENT_STATUS_NORMALIZATION[status] ?? status.toLowerCase();
}

/**
 * The backend serializes OrderStatus enums as PascalCase ("OutForDelivery");
 * the frontend uses lowercase kebab-like values. Normalize every order that
 * crosses the service boundary so components can rely on one shape.
 */
export function normalizeOrderStatus(status: string): OrderStatus {
  return STATUS_NORMALIZATION[status] ?? (status.toLowerCase() as OrderStatus);
}

export function normalizeOrder(order: any): Order {
  if (!order || typeof order !== 'object' || !('status' in order)) return order;
  return {
    ...order,
    status: normalizeOrderStatus(order.status),
    paymentStatus: normalizePaymentStatus(order.paymentStatus),
    customerNote: order.customerNote ?? order.notes,
    items: (order.items ?? []).map((item: any) => ({
      ...item,
      // Backend OrderItemDto serializes the name as `menuItemName`; keep `name` populated
      // so templates can rely on one field regardless of the API shape.
      name: item.name ?? item.menuItemName,
    })),
  };
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  /**
   * Business validation failures arrive as HTTP 200 with { success: false, message, data: null }
   * (see apiResponseInterceptor). Surface them as thrown errors so callers can read `message`.
   */
  private unwrap<T>(value: any): T {
    if (isApiErrorEnvelope(value)) throw value;
    return value as T;
  }

  getCustomerOrders(): Observable<Order[]> {
    return this.http.get<any>(`${this.apiUrl}/customer`).pipe(
      map((res: any) => {
        const list = this.unwrap<any>(res.data || res);
        return Array.isArray(list) ? list.map(o => normalizeOrder(o)) : list;
      })
    );
  }

  getRestaurantOrders(restaurantId: string): Observable<Order[]> {
    return this.http.get<any>(`${this.apiUrl}/restaurant/${restaurantId}`).pipe(
      map((res: any) => {
        const list = this.unwrap<any>(res.data || res);
        return Array.isArray(list) ? list.map(o => normalizeOrder(o)) : list;
      })
    );
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<any>(`${this.apiUrl}/${orderId}`).pipe(
      map((res: any) => normalizeOrder(this.unwrap<Order>(res.data || res)))
    );
  }

  createOrder(data: CreateOrderRequest): Observable<Order | ApiErrorEnvelope> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => normalizeOrder(res.data || res))
    );
  }

  applyCoupon(orderId: string, code: string): Observable<Order | ApiErrorEnvelope> {
    return this.http.post<any>(`${this.apiUrl}/${orderId}/apply-coupon`, { code }).pipe(
      map((res: any) => normalizeOrder(res.data || res))
    );
  }

  updateStatus(orderId: string, status: string): Observable<Order> {
    return this.http.put<any>(`${this.apiUrl}/${orderId}/status`, { status }).pipe(
      map((res: any) => normalizeOrder(this.unwrap<Order>(res.data || res)))
    );
  }

  /**
   * POST /api/orders/{id}/assign-rider — `{}` picks the nearest available rider
   * (backend auto-assign by restaurant proximity within radiusKm); pass `riderId`
   * for manual assignment (no rider-list endpoint exists, so the UI only uses auto).
   */
  assignRider(orderId: string, riderId?: string): Observable<Order> {
    return this.http.post<any>(`${this.apiUrl}/${orderId}/assign-rider`, riderId ? { riderId } : {}).pipe(
      map((res: any) => normalizeOrder(this.unwrap<Order>(res.data || res)))
    );
  }
}