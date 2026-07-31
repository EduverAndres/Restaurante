import { Component, OnInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { OrderService, Order } from '../../../core/services/order.service';
import { SignalrService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-restaurant-orders',
  imports: [DatePipe],
  templateUrl: './restaurant-orders.html',
  styleUrl: './restaurant-orders.css',
})
export class RestaurantOrders implements OnInit, OnDestroy {
  orders: Order[] = [];
  loading = true;
  restaurantId = '';
  filterStatus = 'all';

  newOrderSound = new Audio('/assets/notification.wav');

  constructor(
    private orderService: OrderService,
    private signalr: SignalrService,
    private auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.restaurantId = this.auth.currentUser()?.id || '';
    this.loadOrders();
    this.setupRealtime();
  }

  ngOnDestroy(): void {
    if (this.restaurantId) {
      this.signalr.leaveRestaurantGroup(this.restaurantId);
    }
    this.signalr.stop();
  }

  private loadOrders(): void {
    if (!this.restaurantId) return;
    this.orderService.getRestaurantOrders(this.restaurantId).subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  private setupRealtime(): void {
    this.signalr.start().then(() => {
      if (this.restaurantId) {
        this.signalr.joinRestaurantGroup(this.restaurantId);
      }
    });

    this.signalr.onNewOrder((order) => {
      this.orders.unshift(order);
      this.playNotification();
    });

    this.signalr.onOrderUpdated((updated) => {
      const idx = this.orders.findIndex(o => o.id === updated.id);
      if (idx >= 0) {
        this.orders[idx] = updated;
      }
    });
  }

  private playNotification(): void {
    try {
      this.newOrderSound.play();
    } catch {}
  }

  updateStatus(orderId: string, status: string): void {
    this.orderService.updateStatus(orderId, status).subscribe();
  }

  get filteredOrders(): Order[] {
    if (this.filterStatus === 'all') return this.orders;
    return this.orders.filter(o => o.status === this.filterStatus);
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      pending: 'Pendiente', confirmed: 'Confirmado', preparing: 'Preparando',
      ready: 'Listo', delivered: 'Entregado', cancelled: 'Cancelado',
    };
    return map[status] || status;
  }

  statusBadgeColor(status: string): string {
    const map: Record<string, string> = {
      pending: 'bg-amber-100 text-amber-700',
      confirmed: 'bg-blue-100 text-blue-700',
      preparing: 'bg-orange-100 text-orange-700',
      ready: 'bg-green-100 text-green-700',
      delivered: 'bg-gray-100 text-gray-500',
      cancelled: 'bg-red-100 text-red-700',
    };
    return map[status] || 'bg-gray-100 text-gray-600';
  }

  statusButtonColor(status: string): string {
    const map: Record<string, string> = {
      confirmed: 'bg-blue-500 hover:bg-blue-600',
      preparing: 'bg-orange-500 hover:bg-orange-600',
      ready: 'bg-green-500 hover:bg-green-600',
    };
    return map[status] || 'bg-gray-400';
  }
}
