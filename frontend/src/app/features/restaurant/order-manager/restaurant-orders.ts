import { Component, OnDestroy, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrderService, Order, OrderStatus } from '../../../core/services/order.service';
import { SignalrService } from '../../../core/services/signalr.service';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  ORDER_STATUS_ORDER,
  canCancel,
  formatMoney,
  nextTransitions,
  paymentBadgeClass,
  paymentMethodLabel,
  paymentStatusLabel,
  shortOrderId,
  statusBadgeClass,
  statusLabel,
  transitionButtonClass,
  transitionLabel,
} from '../../../core/utils/restaurant-dashboard';
import { readableApiError } from '../../../core/utils/restaurant-onboarding';

@Component({
  selector: 'app-restaurant-orders',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './restaurant-orders.html',
  styleUrl: './restaurant-orders.css',
})
export class RestaurantOrders implements OnInit, OnDestroy {
  orders: Order[] = [];
  loading = true;
  restaurants: Restaurant[] = [];
  restaurantId = '';
  filterStatus = 'all';
  error = '';

  /** Order waiting for rider assignment (modal). */
  assigningOrder: Order | null = null;
  assigning = false;

  /** Order currently executing a transition (button disabled state). */
  busyOrderId = '';

  newOrderSound = new Audio('/assets/notification.wav');

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private signalr: SignalrService,
    private restaurantService: RestaurantService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    const param = this.route.snapshot.queryParamMap.get('status');
    if (param && ORDER_STATUS_ORDER.includes(param as OrderStatus)) {
      this.filterStatus = param;
    }
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        this.restaurants = restaurants;
        if (restaurants.length > 0) {
          this.restaurantId = restaurants[0].id;
          this.loadOrders();
          this.setupRealtime();
        } else {
          this.loading = false;
        }
      },
      error: (e) => {
        this.error = readableApiError(e, 'No se pudieron cargar tus restaurantes');
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    if (this.restaurantId) {
      this.signalr.leaveRestaurantGroup(this.restaurantId);
    }
  }

  onBranchChange(id: string): void {
    const previous = this.restaurantId;
    this.restaurantId = id;
    if (previous !== this.restaurantId) {
      this.orders = [];
      this.error = '';
      this.signalr.leaveRestaurantGroup(previous);
      this.loadOrders();
      this.signalr.start().then(() => {
        if (this.restaurantId) this.signalr.joinRestaurantGroup(this.restaurantId);
      });
    }
  }

  private loadOrders(): void {
    if (!this.restaurantId) return;
    this.loading = true;
    this.orderService.getRestaurantOrders(this.restaurantId).subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: (e) => {
        this.error = readableApiError(e, 'No se pudieron cargar los pedidos');
        this.loading = false;
      },
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
      this.toast.show(`¡Nuevo pedido #${shortOrderId(order.id)}!`, 'success', 6000);
    });

    this.signalr.onOrderUpdated((updated) => {
      const idx = this.orders.findIndex(o => o.id === updated.id);
      if (idx >= 0) {
        this.orders[idx] = updated;
      } else if (updated.restaurantId === this.restaurantId) {
        this.orders.unshift(updated);
      }
    });
  }

  private playNotification(): void {
    try {
      this.newOrderSound.play();
    } catch {}
  }

  /** Transition buttons for the current status (empty for terminal statuses). */
  actionsFor(order: Order): OrderStatus[] {
    return nextTransitions(order.status);
  }

  runTransition(order: Order, status: OrderStatus): void {
    if (status === 'assignedToRider') {
      this.assigningOrder = order;
      return;
    }
    if (status === 'cancelled' && !confirm('¿Cancelar este pedido?')) return;

    this.busyOrderId = order.id;
    this.orderService.updateStatus(order.id, status).subscribe({
      next: (updated) => {
        const idx = this.orders.findIndex(o => o.id === updated.id);
        if (idx >= 0) this.orders[idx] = updated;
        this.toast.show(`Pedido #${shortOrderId(order.id)}: ${statusLabel(status).toLowerCase()}`, 'success');
        this.busyOrderId = '';
      },
      error: (e) => {
        this.toast.show(readableApiError(e, 'No se pudo actualizar el pedido'), 'error');
        this.busyOrderId = '';
      },
    });
  }

  assignNearestRider(): void {
    if (!this.assigningOrder) return;
    const order = this.assigningOrder;
    this.assigning = true;
    this.orderService.assignRider(order.id).subscribe({
      next: (updated) => {
        const idx = this.orders.findIndex(o => o.id === updated.id);
        if (idx >= 0) this.orders[idx] = updated;
        this.toast.show(`Repartidor asignado al pedido #${shortOrderId(order.id)}`, 'success');
        this.assigningOrder = null;
        this.assigning = false;
      },
      error: (e) => {
        this.toast.show(readableApiError(e, 'No se pudo asignar un repartidor'), 'error');
        this.assigning = false;
      },
    });
  }

  closeAssignModal(): void {
    if (this.assigning) return;
    this.assigningOrder = null;
  }

  get filteredOrders(): Order[] {
    if (this.filterStatus === 'all') return this.orders;
    return this.orders.filter(o => o.status === this.filterStatus);
  }

  protected readonly statusLabel = statusLabel;
  protected readonly statusBadgeClass = statusBadgeClass;
  protected readonly transitionButtonClass = transitionButtonClass;
  protected readonly transitionLabel = transitionLabel;
  protected readonly paymentStatusLabel = paymentStatusLabel;
  protected readonly paymentBadgeClass = paymentBadgeClass;
  protected readonly paymentMethodLabel = paymentMethodLabel;
  protected readonly formatMoney = formatMoney;
  protected readonly canCancel = canCancel;
  protected readonly shortOrderId = shortOrderId;
  protected readonly filterTabs = ['all', ...ORDER_STATUS_ORDER];
}
