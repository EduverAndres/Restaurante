import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { SignalrService } from '../../../core/services/signalr.service';
import { Restaurant, RestaurantDashboard, RestaurantService } from '../../../core/services/restaurant.service';
import {
  formatMoney,
  formatPrepTime,
  orderCountsArray,
  shortOrderId,
  statusBadgeClass,
  statusLabel,
  totalFromCounts,
  StatusCount,
} from '../../../core/utils/restaurant-dashboard';
import { readableApiError } from '../../../core/utils/restaurant-onboarding';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  protected auth: AuthService;

  restaurants: Restaurant[] = [];
  selectedId = '';
  selected: Restaurant | null = null;
  dashboard: RestaurantDashboard | null = null;
  loading = true;
  dataLoading = false;
  hasRestaurant = false;
  error = '';
  lastUpdated: Date | null = null;

  private refreshTimer: ReturnType<typeof setInterval> | null = null;

  constructor(
    auth: AuthService,
    private restaurantService: RestaurantService,
    private signalr: SignalrService,
    private toast: ToastService,
  ) {
    this.auth = auth;
  }

  ngOnInit(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        this.restaurants = restaurants;
        if (restaurants.length > 0) {
          this.hasRestaurant = true;
          this.selectRestaurant(restaurants[0]);
        }
        this.loading = false;
      },
      error: (e) => {
        this.error = readableApiError(e, 'No se pudieron cargar tus restaurantes');
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.stopAutoRefresh();
    if (this.selectedId) {
      this.signalr.leaveRestaurantGroup(this.selectedId);
    }
  }

  /** Switches branch: reload data, rejoin SignalR group and restart auto-refresh. */
  selectRestaurant(restaurant: Restaurant): void {
    const previousId = this.selectedId;
    this.selectedId = restaurant.id;
    this.selected = restaurant;
    this.dashboard = null;
    this.error = '';
    this.refresh();
    this.setupRealtime(previousId);
    this.startAutoRefresh();
  }

  onBranchChange(id: string): void {
    const next = this.restaurants.find(r => r.id === id);
    if (next) this.selectRestaurant(next);
  }

  refresh(): void {
    if (!this.selectedId) return;
    this.dataLoading = true;
    this.restaurantService.getDashboard(this.selectedId).subscribe({
      next: (data) => {
        this.dashboard = data;
        this.dataLoading = false;
        this.lastUpdated = new Date();
        this.error = '';
      },
      error: (e) => {
        this.dataLoading = false;
        this.error = readableApiError(e, 'No se pudo cargar el dashboard');
      },
    });
  }

  get statusCounts(): StatusCount[] {
    return orderCountsArray(this.dashboard?.orderCountsByStatus);
  }

  get totalOrders(): number {
    return this.dashboard?.totalOrders ?? totalFromCounts(this.dashboard?.orderCountsByStatus);
  }

  protected readonly formatMoney = formatMoney;
  protected readonly formatPrepTime = formatPrepTime;
  protected readonly statusLabel = statusLabel;
  protected readonly statusBadgeClass = statusBadgeClass;
  protected readonly shortOrderId = shortOrderId;

  private setupRealtime(previousId: string): void {
    if (previousId && previousId !== this.selectedId) {
      this.signalr.leaveRestaurantGroup(previousId);
    }
    this.signalr.start().then(() => {
      if (this.selectedId) this.signalr.joinRestaurantGroup(this.selectedId);
    });
    this.signalr.onNewOrder((order) => {
      this.toast.show(`¡Nuevo pedido #${shortOrderId(order.id)}!`, 'success', 6000);
      this.refresh();
    });
    this.signalr.onOrderUpdated(() => this.refresh());
  }

  private startAutoRefresh(): void {
    this.stopAutoRefresh();
    this.refreshTimer = setInterval(() => this.refresh(), 60_000);
  }

  private stopAutoRefresh(): void {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
      this.refreshTimer = null;
    }
  }
}
