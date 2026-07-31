import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService, Order } from '../../../core/services/order.service';
import { SignalrService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, DatePipe],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.css',
})
export class OrderDetail implements OnInit, OnDestroy {
  order: Order | null = null;
  loading = true;

  readonly statusSteps = ['pending', 'confirmed', 'preparing', 'ready', 'delivered', 'cancelled'] as const;

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private signalr: SignalrService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(id);
      this.signalr.start();
      this.signalr.onOrderUpdated((updated) => {
        if (updated.id === id) {
          this.order = updated;
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.signalr.stop();
  }

  private loadOrder(id: string): void {
    this.orderService.getOrderById(id).subscribe({
      next: (data) => {
        this.order = data;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  get currentStepIndex(): number {
    if (!this.order) return 0;
    const idx = this.statusSteps.indexOf(this.order.status);
    return idx >= 0 ? idx : 0;
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      pending: 'Pendiente',
      confirmed: 'Confirmado',
      preparing: 'Preparando',
      ready: 'Listo para recoger',
      delivered: 'Entregado',
      cancelled: 'Cancelado',
    };
    return map[status] || status;
  }

  statusColor(status: string): string {
    const map: Record<string, string> = {
      pending: 'bg-amber-400',
      confirmed: 'bg-blue-500',
      preparing: 'bg-orange-500',
      ready: 'bg-green-500',
      delivered: 'bg-gray-400',
      cancelled: 'bg-red-500',
    };
    return map[status] || 'bg-gray-300';
  }
}
