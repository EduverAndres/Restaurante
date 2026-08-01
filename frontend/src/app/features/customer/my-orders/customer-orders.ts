import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService, Order } from '../../../core/services/order.service';
import { SignalrService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-customer-orders',
  imports: [RouterLink, DatePipe],
  templateUrl: './customer-orders.html',
  styleUrl: './customer-orders.css',
})
export class CustomerOrders implements OnInit, OnDestroy {
  orders: Order[] = [];
  loading = true;

  constructor(
    private orderService: OrderService,
    private signalr: SignalrService,
  ) {}

  ngOnInit(): void {
    this.signalr.start();
    this.signalr.onOrderUpdated((updated) => {
      const idx = this.orders.findIndex(o => o.id === updated.id);
      if (idx >= 0) {
        this.orders[idx] = updated;
      }
    });
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.leaveActiveOrderGroups();
    this.signalr.stop();
  }

  private joinActiveOrderGroups(): void {
    const activeStatuses = ['pending', 'confirmed', 'preparing', 'ready'];
    this.orders
      .filter(o => activeStatuses.includes(o.status))
      .forEach(o => this.signalr.joinOrderGroup(o.id));
  }

  private leaveActiveOrderGroups(): void {
    this.orders.forEach(o => this.signalr.leaveOrderGroup(o.id));
  }

  private loadOrders(): void {
    this.orderService.getCustomerOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
        // Join order groups after orders are loaded
        this.signalr.start().then(() => this.joinActiveOrderGroups());
      },
      error: () => (this.loading = false),
    });
  }

  statusColor(status: string): string {
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

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      pending: 'Pendiente',
      confirmed: 'Confirmado',
      preparing: 'Preparando',
      ready: 'Listo',
      delivered: 'Entregado',
      cancelled: 'Cancelado',
    };
    return map[status] || status;
  }
}
