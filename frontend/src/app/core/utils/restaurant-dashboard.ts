import { OrderStatus } from '../services/order.service';

/** Human-readable Spanish labels for every order status (lowercase frontend convention). */
export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  pending: 'Pendiente',
  confirmed: 'Confirmado',
  preparing: 'Preparando',
  ready: 'Listo',
  assignedToRider: 'Con repartidor',
  outForDelivery: 'En reparto',
  delivered: 'Entregado',
  cancelled: 'Cancelado',
};

/** Canonical order of statuses, matching the backend OrderStatus enum order. */
export const ORDER_STATUS_ORDER: OrderStatus[] = [
  'pending',
  'confirmed',
  'preparing',
  'ready',
  'assignedToRider',
  'outForDelivery',
  'delivered',
  'cancelled',
];

export function statusLabel(status: string): string {
  return ORDER_STATUS_LABELS[status as OrderStatus] ?? status;
}

/** Badge class for a status; styles.css defines `.badge-<status>` for each one. */
export function statusBadgeClass(status: string): string {
  return `badge-${status}`;
}

/**
 * Allowed restaurant-side transitions, mirroring the backend state machine
 * (UpdateOrderStatusCommand.IsValidTransition). Delivered and Cancelled are terminal.
 */
export const RESTAURANT_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  pending: ['confirmed', 'cancelled'],
  confirmed: ['preparing', 'cancelled'],
  preparing: ['ready', 'cancelled'],
  ready: ['assignedToRider', 'cancelled'],
  assignedToRider: ['outForDelivery', 'cancelled'],
  outForDelivery: ['delivered', 'cancelled'],
  delivered: [],
  cancelled: [],
};

export function nextTransitions(status: OrderStatus): OrderStatus[] {
  return RESTAURANT_TRANSITIONS[status] ?? [];
}

/** The backend only allows rider assignment while the order is Ready (or re-assign while AssignedToRider). */
export function canAssignRider(status: OrderStatus): boolean {
  return status === 'ready' || status === 'assignedToRider';
}

export function canCancel(status: OrderStatus): boolean {
  return RESTAURANT_TRANSITIONS[status]?.includes('cancelled') ?? false;
}

export const TRANSITION_LABELS: Partial<Record<OrderStatus, string>> = {
  confirmed: 'Confirmar',
  preparing: 'Preparar',
  ready: 'Listo',
  assignedToRider: 'Asignar repartidor',
  outForDelivery: 'En reparto',
  delivered: 'Entregado',
  cancelled: 'Cancelar',
};

export function transitionLabel(status: OrderStatus): string {
  return TRANSITION_LABELS[status] ?? status;
}

/** Tailwind classes for the transition action button (restaurant order card). */
export function transitionButtonClass(status: OrderStatus): string {
  const base = 'btn btn-sm text-white';
  switch (status) {
    case 'confirmed':
      return `${base} bg-blue-500 hover:bg-blue-600`;
    case 'preparing':
      return `${base} bg-orange-500 hover:bg-orange-600`;
    case 'ready':
      return `${base} bg-green-500 hover:bg-green-600`;
    case 'assignedToRider':
      return `${base} bg-purple-500 hover:bg-purple-600`;
    case 'outForDelivery':
      return `${base} bg-sky-500 hover:bg-sky-600`;
    case 'delivered':
      return `${base} bg-emerald-500 hover:bg-emerald-600`;
    case 'cancelled':
      return 'btn btn-outline btn-sm';
    default:
      return base;
  }
}

/** Money with `$` and thousands separator, matching the rest of the UI. */
export function formatMoney(value: number | null | undefined): string {
  if (value == null) return '$0.00';
  return `$${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

export function formatPrepTime(minutes: number | null | undefined): string {
  if (minutes == null) return '—';
  return `${Math.round(minutes)} min`;
}

export function shortOrderId(id: string): string {
  return id.slice(0, 8);
}

export interface StatusCount {
  status: OrderStatus;
  count: number;
}

/** Flatten `orderCountsByStatus` into an array in canonical status order, dropping zero counts. */
export function orderCountsArray(counts: Record<string, number> | null | undefined): StatusCount[] {
  if (!counts) return [];
  return ORDER_STATUS_ORDER.map(status => ({ status, count: counts[status] ?? 0 })).filter(c => c.count > 0);
}

export function totalFromCounts(counts: Record<string, number> | null | undefined): number {
  if (!counts) return 0;
  return Object.values(counts).reduce((sum, n) => sum + n, 0);
}

const PAYMENT_LABELS: Record<string, string> = {
  pending: 'Pendiente',
  paid: 'Pagado',
  refunded: 'Reembolsado',
  failed: 'Fallido',
};

export function paymentStatusLabel(status: string | null | undefined): string {
  return PAYMENT_LABELS[(status ?? '').toLowerCase()] ?? '—';
}

/** Human-readable Spanish label for the backend PaymentMethod ("CASH"/"CARD"). */
const PAYMENT_METHOD_LABELS: Record<string, string> = {
  CASH: 'Efectivo',
  CARD: 'Tarjeta',
};

export function paymentMethodLabel(method: string | null | undefined): string {
  return PAYMENT_METHOD_LABELS[(method ?? '').toUpperCase()] ?? '—';
}

export function paymentBadgeClass(status: string | null | undefined): string {
  switch ((status ?? '').toLowerCase()) {
    case 'paid':
      return 'badge-delivered';
    case 'refunded':
    case 'failed':
      return 'badge-cancelled';
    case 'pending':
    default:
      return 'badge-pending';
  }
}
