import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService, Order } from '../../../core/services/order.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  orders: Order[] = [];
  loading = true;
  restaurantId = '';

  constructor(
    protected auth: AuthService,
    private orderService: OrderService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.restaurantId = restaurants[0].id;
          this.loadOrders();
        } else {
          this.loading = false;
        }
      },
      error: () => (this.loading = false),
    });
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

  get todayOrders(): Order[] {
    const today = new Date().toDateString();
    return this.orders.filter(o => new Date(o.createdAt).toDateString() === today);
  }

  get pendingOrders(): Order[] {
    return this.orders.filter(o => o.status === 'pending' || o.status === 'confirmed');
  }

  get todayRevenue(): number {
    return this.todayOrders
      .filter(o => o.status !== 'cancelled')
      .reduce((sum, o) => sum + o.total, 0);
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      pending: 'Pendiente', confirmed: 'Confirmado', preparing: 'Preparando',
      ready: 'Listo', delivered: 'Entregado', cancelled: 'Cancelado',
    };
    return map[status] || status;
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
}
