import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService, Order } from '../../../core/services/order.service';
import { SignalrService } from '../../../core/services/signalr.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-customer-orders',
  imports: [RouterLink, DatePipe],
  templateUrl: './customer-orders.html',
  styleUrl: './customer-orders.css',
})
export class CustomerOrders implements OnInit, OnDestroy {
  orders: Order[] = [];
  loading = true;
  reorderingId = signal<string | null>(null);

  constructor(
    private orderService: OrderService,
    private signalr: SignalrService,
    private restaurantService: RestaurantService,
    private cartService: CartService,
    private toast: ToastService,
    private router: Router,
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
      error: () => {
        this.loading = false;
        this.toast.show('No se pudieron cargar tus pedidos', 'error');
      },
    });
  }

  /**
   * "Pedir de nuevo": reload the delivered order, validate each item against
   * the current menu (skip unavailable ones) and rebuild the cart.
   */
  reorder(order: Order): void {
    if (this.reorderingId()) return;
    this.reorderingId.set(order.id);

    this.orderService.getOrderById(order.id).subscribe({
      next: (full) => {
        if (!full || !full.restaurantId) {
          this.reorderFailed();
          return;
        }
        this.restaurantService.getById(full.restaurantId).subscribe({
          next: (restaurant) => {
            if (!restaurant || !restaurant.id) {
              this.reorderFailed();
              return;
            }
            this.restaurantService.getMenu(restaurant.id).subscribe({
              next: (menu) => {
                const menuItems = menu.flatMap(c => c.items);
                const byId = new Map(menuItems.map(i => [i.id, i]));
                const byName = new Map(menuItems.map(i => [i.name.toLowerCase(), i]));

                this.cartService.setRestaurant(restaurant);
                let skipped = 0;
                for (const item of full.items) {
                  const menuItem =
                    (item.menuItemId && byId.get(item.menuItemId)) ||
                    (item.name && byName.get(item.name.toLowerCase()));
                  if (!menuItem || !menuItem.isAvailable) {
                    skipped++;
                    continue;
                  }
                  this.cartService.addItem(menuItem, item.quantity, item.notes);
                }

                this.reorderingId.set(null);
                if (skipped > 0) {
                  this.toast.show(
                    `Carrito actualizado (${skipped} ítem(s) ya no disponible(s) fueron omitidos)`,
                    'info',
                  );
                } else {
                  this.toast.show('Carrito actualizado', 'success');
                }
                this.router.navigate(['/restaurant', restaurant.slug]);
              },
              error: () => this.reorderFailed(),
            });
          },
          error: () => this.reorderFailed(),
        });
      },
      error: () => this.reorderFailed(),
    });
  }

  private reorderFailed(): void {
    this.reorderingId.set(null);
    this.toast.show('No se pudo pedir de nuevo', 'error');
  }

  statusColor(status: string): string {
    const map: Record<string, string> = {
      pending: 'bg-amber-100 text-amber-700',
      confirmed: 'bg-blue-100 text-blue-700',
      preparing: 'bg-orange-100 text-orange-700',
      ready: 'bg-green-100 text-green-700',
      assignedToRider: 'bg-purple-100 text-purple-700',
      outForDelivery: 'bg-sky-100 text-sky-700',
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
      assignedToRider: 'Repartidor asignado',
      outForDelivery: 'En camino',
      delivered: 'Entregado',
      cancelled: 'Cancelado',
    };
    return map[status] || status;
  }
}
