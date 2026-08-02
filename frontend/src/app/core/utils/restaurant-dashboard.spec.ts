import { describe, expect, it } from 'vitest';
import {
  ORDER_STATUS_LABELS,
  ORDER_STATUS_ORDER,
  RESTAURANT_TRANSITIONS,
  canAssignRider,
  canCancel,
  formatMoney,
  formatPrepTime,
  nextTransitions,
  orderCountsArray,
  paymentBadgeClass,
  paymentStatusLabel,
  shortOrderId,
  statusBadgeClass,
  statusLabel,
  totalFromCounts,
  transitionButtonClass,
  transitionLabel,
} from './restaurant-dashboard';
import { OrderStatus } from '../services/order.service';

describe('statusLabel / statusBadgeClass', () => {
  it('labels every canonical status in Spanish', () => {
    for (const status of ORDER_STATUS_ORDER) {
      expect(ORDER_STATUS_LABELS[status]).toBeTruthy();
    }
    expect(statusLabel('pending')).toBe('Pendiente');
    expect(statusLabel('assignedToRider')).toBe('Con repartidor');
    expect(statusLabel('unknown-status')).toBe('unknown-status');
  });

  it('maps status to its semantic badge class', () => {
    expect(statusBadgeClass('outForDelivery')).toBe('badge-outForDelivery');
    expect(statusBadgeClass('cancelled')).toBe('badge-cancelled');
  });
});

describe('state machine helpers', () => {
  it('mirrors the backend transitions', () => {
    expect(nextTransitions('pending')).toEqual(['confirmed', 'cancelled']);
    expect(nextTransitions('confirmed')).toEqual(['preparing', 'cancelled']);
    expect(nextTransitions('preparing')).toEqual(['ready', 'cancelled']);
    expect(nextTransitions('ready')).toEqual(['assignedToRider', 'cancelled']);
    expect(nextTransitions('assignedToRider')).toEqual(['outForDelivery', 'cancelled']);
    expect(nextTransitions('outForDelivery')).toEqual(['delivered', 'cancelled']);
    expect(nextTransitions('delivered')).toEqual([]);
    expect(nextTransitions('cancelled')).toEqual([]);
  });

  it('never allows same-status or terminal transitions', () => {
    for (const status of ORDER_STATUS_ORDER) {
      expect(nextTransitions(status)).not.toContain(status);
    }
    expect(RESTAURANT_TRANSITIONS.delivered).toHaveLength(0);
  });

  it('only allows rider assignment on ready/assignedToRider', () => {
    expect(canAssignRider('ready')).toBe(true);
    expect(canAssignRider('assignedToRider')).toBe(true);
    expect(canAssignRider('preparing')).toBe(false);
    expect(canAssignRider('delivered')).toBe(false);
  });

  it('cancellable only while not terminal', () => {
    expect(canCancel('pending')).toBe(true);
    expect(canCancel('outForDelivery')).toBe(true);
    expect(canCancel('delivered')).toBe(false);
    expect(canCancel('cancelled')).toBe(false);
  });

  it('gives every transition a label and button class', () => {
    expect(transitionLabel('confirmed')).toBe('Confirmar');
    expect(transitionLabel('cancelled')).toBe('Cancelar');
    expect(transitionButtonClass('confirmed')).toContain('bg-blue-500');
    expect(transitionButtonClass('cancelled')).toContain('btn-outline');
  });
});

describe('formatMoney', () => {
  it('formats with two decimals and thousands separator', () => {
    expect(formatMoney(1234.5)).toBe('$1,234.50');
    expect(formatMoney(0)).toBe('$0.00');
    expect(formatMoney(99)).toBe('$99.00');
  });

  it('handles null/undefined', () => {
    expect(formatMoney(null)).toBe('$0.00');
    expect(formatMoney(undefined)).toBe('$0.00');
  });
});

describe('formatPrepTime', () => {
  it('rounds minutes and shows em dash when unknown', () => {
    expect(formatPrepTime(12.4)).toBe('12 min');
    expect(formatPrepTime(12.6)).toBe('13 min');
    expect(formatPrepTime(null)).toBe('—');
    expect(formatPrepTime(undefined)).toBe('—');
  });
});

describe('shortOrderId', () => {
  it('truncates a guid to 8 chars', () => {
    expect(shortOrderId('12345678-9abc-def0-1234-56789abcdef0')).toBe('12345678');
  });
});

describe('orderCountsArray / totalFromCounts', () => {
  it('flattens counts in canonical order and drops zeros', () => {
    const counts = { delivered: 5, pending: 2, cancelled: 0, preparing: 1 };
    expect(orderCountsArray(counts)).toEqual([
      { status: 'pending', count: 2 },
      { status: 'preparing', count: 1 },
      { status: 'delivered', count: 5 },
    ]);
  });

  it('returns empty for null/empty input', () => {
    expect(orderCountsArray(null)).toEqual([]);
    expect(orderCountsArray({})).toEqual([]);
  });

  it('totals all statuses', () => {
    expect(totalFromCounts({ pending: 2, delivered: 5, cancelled: 1 })).toBe(8);
    expect(totalFromCounts(null)).toBe(0);
  });
});

describe('payment helpers', () => {
  it('labels payment statuses (PascalCase from backend)', () => {
    expect(paymentStatusLabel('Paid')).toBe('Pagado');
    expect(paymentStatusLabel('pending')).toBe('Pendiente');
    expect(paymentStatusLabel('Refunded')).toBe('Reembolsado');
    expect(paymentStatusLabel('Failed')).toBe('Fallido');
    expect(paymentStatusLabel(undefined)).toBe('—');
  });

  it('maps payment status to badge classes', () => {
    expect(paymentBadgeClass('Paid')).toBe('badge-delivered');
    expect(paymentBadgeClass('pending')).toBe('badge-pending');
    expect(paymentBadgeClass('Refunded')).toBe('badge-cancelled');
    expect(paymentBadgeClass('Failed')).toBe('badge-cancelled');
  });
});

describe('transition type safety', () => {
  it('every transition target is a valid OrderStatus', () => {
    const statuses = new Set<OrderStatus>(ORDER_STATUS_ORDER);
    for (const targets of Object.values(RESTAURANT_TRANSITIONS)) {
      for (const target of targets) {
        expect(statuses.has(target)).toBe(true);
      }
    }
  });
});
