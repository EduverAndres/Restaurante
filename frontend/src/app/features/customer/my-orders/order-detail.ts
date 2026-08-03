import { Component, OnInit, OnDestroy, ElementRef, effect, signal, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as L from 'leaflet';
import { OrderService, Order, OrderStatus } from '../../../core/services/order.service';
import { SignalrService, RiderLocation } from '../../../core/services/signalr.service';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';
import { AddressService } from '../../../core/services/address.service';
import { ReviewService } from '../../../core/services/review.service';
import { ToastService } from '../../../core/services/toast.service';
import { isApiErrorEnvelope } from '../../../core/services/api-response';

interface LatLng {
  lat: number;
  lng: number;
}

const DELIVERY_STATUSES = new Set(['assignedToRider', 'outForDelivery']);

const STATUS_TOAST_MESSAGE: Record<string, string> = {
  pending: 'Pedido recibido',
  confirmed: 'Pedido confirmado',
  preparing: 'En preparación',
  ready: 'Listo para recoger',
  assignedToRider: 'Repartidor asignado',
  outForDelivery: 'Pedido en camino',
  delivered: 'Pedido entregado',
  cancelled: 'Pedido cancelado',
};

/** Average rider ground speed used for the ETA estimate (m/min). */
const RIDER_SPEED_M_PER_MIN = 300;

function haversineMeters(a: LatLng, b: LatLng): number {
  const R = 6371000;
  const toRad = (deg: number) => (deg * Math.PI) / 180;
  const dLat = toRad(b.lat - a.lat);
  const dLng = toRad(b.lng - a.lng);
  const s =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(a.lat)) * Math.cos(toRad(b.lat)) * Math.sin(dLng / 2) ** 2;
  return 2 * R * Math.asin(Math.sqrt(s));
}

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.css',
})
export class OrderDetail implements OnInit, OnDestroy {
  order: Order | null = null;
  loading = true;

  /** Review form state. */
  rating = signal(0);
  comment = '';
  submittingReview = false;
  reviewThanks = false;

  /** Live tracking state. */
  showMap = signal(false);
  showOnTheWay = false;
  etaMinutes: number | null = null;
  hasRiderPosition = signal(false);

  readonly statusSteps: OrderStatus[] = [
    'pending',
    'confirmed',
    'preparing',
    'ready',
    'assignedToRider',
    'outForDelivery',
    'delivered',
  ];

  private orderId = '';
  private previousStatus: string | null = null;
  private map: L.Map | null = null;
  private riderMarker: L.Marker | null = null;
  private restaurantCoords: LatLng | null = null;
  private destinationCoords: LatLng | null = null;
  private destinationResolved = false;
  private riderPosition: LatLng | null = null;
  private prepTimeMinutes: number | null = null;
  private readonly restaurantCache = new Map<string, Restaurant>();
  private readonly mapElement = viewChild.required<ElementRef<HTMLDivElement>>('trackingMap');

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private signalr: SignalrService,
    private restaurantService: RestaurantService,
    private addressService: AddressService,
    private reviewService: ReviewService,
    private toast: ToastService,
  ) {
    effect(() => {
      if (this.showMap() && !this.map) {
        const el = this.mapElement();
        if (el) this.initMap(el.nativeElement);
      }
    });
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.orderId) return;

    this.signalr.start().then(() => this.signalr.joinOrderGroup(this.orderId));
    this.signalr.onOrderUpdated((updated) => this.onOrderUpdated(updated));
    this.signalr.onRiderLocationUpdated((location) => this.onRiderLocationUpdated(location));
    this.loadOrder(this.orderId);
  }

  ngOnDestroy(): void {
    this.destroyMap();
    if (this.orderId) {
      this.signalr.leaveOrderGroup(this.orderId);
    }
    this.signalr.stop();
  }

  private loadOrder(id: string): void {
    this.orderService.getOrderById(id).subscribe({
      next: (data) => {
        this.order = data;
        this.previousStatus = data.status;
        this.loading = false;
        this.prepareTracking();
      },
      error: () => {
        this.loading = false;
        this.toast.show('No se pudo cargar el pedido', 'error');
      },
    });
  }

  private onOrderUpdated(updated: Order): void {
    if (updated.id !== this.orderId) return;
    const was = this.previousStatus;
    this.order = updated;
    this.previousStatus = updated.status;
    if (was && was !== updated.status) {
      const message = STATUS_TOAST_MESSAGE[updated.status];
      if (message) this.toast.show(message, 'info');
    }
    this.prepareTracking();
  }

  private onRiderLocationUpdated(location: RiderLocation): void {
    if (location.orderId !== this.orderId) return;
    this.riderPosition = { lat: location.latitude, lng: location.longitude };
    this.hasRiderPosition.set(true);

    if (this.map && this.riderMarker) {
      this.riderMarker.setLatLng(this.riderPosition);
      this.fitBounds();
    } else if (!this.map && this.showOnTheWay) {
      // First rider ping: promote the "on the way" card to a live map (rider only).
      this.showOnTheWay = false;
      this.showMap.set(true);
    }
    this.computeEta();
  }

  private prepareTracking(): void {
    if (!this.order || !DELIVERY_STATUSES.has(this.order.status)) {
      this.destroyMap();
      this.showMap.set(false);
      this.showOnTheWay = false;
      return;
    }

    this.resolveDestinationCoords().then((dest) => {
      this.destinationCoords = dest;
      this.resolveRestaurantCoords().then(() => {
        this.computeEta();

        const hasAnyCoords =
          !!this.destinationCoords || !!this.restaurantCoords || !!this.riderPosition;
        if (hasAnyCoords) {
          this.showOnTheWay = false;
          this.showMap.set(true);
        } else {
          this.showOnTheWay = true;
        }
      });
    });
  }

  private resolveDestinationCoords(): Promise<LatLng | null> {
    const order = this.order;
    if (!order) return Promise.resolve(null);
    if (typeof order.latitude === 'number' && typeof order.longitude === 'number') {
      return Promise.resolve({ lat: order.latitude, lng: order.longitude });
    }
    if (this.destinationResolved) return Promise.resolve(this.destinationCoords);
    this.destinationResolved = true;
    // OrderDto has no coordinates: fall back to the customer's default address.
    return new Promise((resolve) => {
      this.addressService.getAddresses().subscribe({
        next: (addresses) => {
          const pick =
            addresses.find(a => a.isDefault && typeof a.latitude === 'number' && typeof a.longitude === 'number') ??
            addresses.find(a => typeof a.latitude === 'number' && typeof a.longitude === 'number');
          resolve(pick && pick.latitude != null && pick.longitude != null
            ? { lat: pick.latitude, lng: pick.longitude }
            : null);
        },
        error: () => resolve(null),
      });
    });
  }

  private resolveRestaurantCoords(): Promise<void> {
    const order = this.order;
    if (!order) return Promise.resolve();
    const cached = this.restaurantCache.get(order.restaurantId);
    if (cached) {
      this.applyRestaurant(cached);
      return Promise.resolve();
    }
    return new Promise((resolve) => {
      this.restaurantService.getById(order.restaurantId).subscribe({
        next: (restaurant) => {
          this.restaurantCache.set(order.restaurantId, restaurant);
          this.applyRestaurant(restaurant);
          resolve();
        },
        error: () => resolve(),
      });
    });
  }

  private applyRestaurant(restaurant: Restaurant): void {
    if (typeof restaurant.latitude === 'number' && typeof restaurant.longitude === 'number') {
      this.restaurantCoords = { lat: restaurant.latitude, lng: restaurant.longitude };
    }
    this.prepTimeMinutes = restaurant.estimatedPrepTimeMinutes ?? null;
  }

  private computeEta(): void {
    let minutes = this.prepTimeMinutes ?? 0;
    if (this.riderPosition && this.destinationCoords) {
      minutes += haversineMeters(this.riderPosition, this.destinationCoords) / RIDER_SPEED_M_PER_MIN;
    }
    this.etaMinutes = minutes > 0 ? Math.round(minutes) : null;
  }

  private initMap(container: HTMLElement): void {
    const center = this.destinationCoords ?? this.restaurantCoords ?? this.riderPosition;
    if (!center) return;

    this.map = L.map(container, {
      center: [center.lat, center.lng],
      zoom: 14,
      scrollWheelZoom: false,
    });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19,
    }).addTo(this.map);

    if (this.restaurantCoords) {
      L.marker([this.restaurantCoords.lat, this.restaurantCoords.lng], {
        icon: L.divIcon({
          className: '',
          html: '<div class="tracking-marker tracking-marker-restaurant" title="Restaurante">R</div>',
          iconSize: [34, 34],
          iconAnchor: [17, 17],
        }),
      })
        .bindTooltip('Restaurante')
        .addTo(this.map);
    }

    if (this.destinationCoords) {
      L.marker([this.destinationCoords.lat, this.destinationCoords.lng], {
        icon: L.divIcon({
          className: '',
          html: '<div class="tracking-marker tracking-marker-destination" title="Tu dirección">D</div>',
          iconSize: [34, 34],
          iconAnchor: [17, 17],
        }),
      })
        .bindTooltip('Tu dirección')
        .addTo(this.map);
    }

    if (this.riderPosition) {
      this.riderMarker = L.marker([this.riderPosition.lat, this.riderPosition.lng], {
        icon: L.divIcon({
          className: '',
          html: '<div class="tracking-marker tracking-marker-rider" title="Repartidor"></div>',
          iconSize: [34, 34],
          iconAnchor: [17, 17],
        }),
      })
        .bindTooltip('Repartidor')
        .addTo(this.map);
    }

    this.fitBounds();
  }

  private fitBounds(): void {
    if (!this.map) return;
    const points = [
      this.restaurantCoords,
      this.destinationCoords,
      this.riderPosition,
    ].filter((p): p is LatLng => !!p);
    if (points.length === 0) return;
    if (points.length === 1) {
      this.map.setView([points[0].lat, points[0].lng], 14);
    } else {
      this.map.fitBounds(points.map(p => [p.lat, p.lng] as [number, number]), {
        padding: [40, 40],
      });
    }
  }

  private destroyMap(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
      this.riderMarker = null;
    }
  }

  /* ---------- Review form ---------- */

  setRating(value: number): void {
    this.rating.set(value);
  }

  submitReview(): void {
    const order = this.order;
    if (!order || this.rating() < 1 || this.submittingReview) return;

    this.submittingReview = true;
    this.reviewService
      .createReview(order.restaurantId, {
        orderId: order.id,
        rating: this.rating(),
        comment: this.comment.trim() ? this.comment.trim() : undefined,
      })
      .subscribe({
        next: (res) => {
          this.submittingReview = false;
          if (isApiErrorEnvelope(res)) {
            const message =
              res.message === 'This order has already been reviewed'
                ? 'Ya calificaste este pedido'
                : res.message;
            this.toast.show(message, 'error');
            return;
          }
          this.reviewThanks = true;
          this.toast.show('¡Gracias por tu reseña!', 'success');
        },
        error: (err) => {
          this.submittingReview = false;
          const message =
            typeof err?.error?.message === 'string' ? err.error.message : 'No se pudo enviar la reseña';
          this.toast.show(message, 'error');
        },
      });
  }

  /* ---------- Presentational helpers ---------- */

  get destinationCoordsVisible(): boolean {
    return !!this.destinationCoords;
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
      assignedToRider: 'Repartidor asignado',
      outForDelivery: 'En camino',
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
      assignedToRider: 'bg-purple-500',
      outForDelivery: 'bg-sky-500',
      delivered: 'bg-gray-400',
      cancelled: 'bg-red-500',
    };
    return map[status] || 'bg-gray-300';
  }
}
