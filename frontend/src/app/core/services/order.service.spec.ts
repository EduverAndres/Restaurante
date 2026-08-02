import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OrderService, normalizeOrder, normalizeOrderStatus } from './order.service';
import { apiResponseInterceptor } from '../interceptors/api-response.interceptor';

const apiOrder = {
  id: 'o1',
  customerId: 'c1',
  restaurantId: 'r1',
  restaurantName: 'La Casa del Taco',
  customerName: 'María García',
  items: [
    { menuItemId: 'm1', menuItemName: 'Tacos al Pastor', quantity: 2, unitPrice: 89 },
  ],
  total: 223,
  deliveryFee: 45,
  discountAmount: 0,
  couponId: null,
  status: 'OutForDelivery',
  notes: null,
  paymentStatus: 'Paid',
  createdAt: '2026-08-01T18:00:00Z',
  updatedAt: '2026-08-01T19:00:00Z',
};

const envelope = (data: unknown) => ({ success: true, message: 'ok', data, errors: null });

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiResponseInterceptor])),
        provideHttpClientTesting(),
        OrderService,
      ],
    });

    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should GET /api/orders/customer and normalize each order', () => {
    service.getCustomerOrders().subscribe((res) => {
      expect(res).toHaveLength(1);
      expect(res[0].status).toBe('outForDelivery');
      expect(res[0].items[0].name).toBe('Tacos al Pastor');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/orders/customer');
    expect(req.request.method).toBe('GET');
    req.flush(envelope([apiOrder]));
  });

  it('should POST /api/orders with restaurantId and items on createOrder', () => {
    service.createOrder({
      restaurantId: 'r1',
      items: [{ menuItemId: 'm1', quantity: 2 }],
      customerNote: 'Sin cebolla',
    }).subscribe((res) => {
      expect(res.id).toBe('o1');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/orders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      restaurantId: 'r1',
      items: [{ menuItemId: 'm1', quantity: 2 }],
      customerNote: 'Sin cebolla',
    });
    req.flush(envelope(apiOrder));
  });

  it('should POST /api/orders/:id/apply-coupon with the code', () => {
    service.applyCoupon('o1', 'WELCOME10').subscribe((res) => {
      expect(res.id).toBe('o1');
      expect(res.total).toBe(223);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/orders/o1/apply-coupon');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ code: 'WELCOME10' });
    req.flush(envelope(apiOrder));
  });

  it('should PUT /api/orders/:id/status on updateStatus', () => {
    service.updateStatus('o1', 'Delivered').subscribe((res) => {
      expect(res.status).toBe('outForDelivery');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/orders/o1/status');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ status: 'Delivered' });
    req.flush(envelope(apiOrder));
  });

  it('should POST /api/orders/:id/assign-rider with empty body for auto-assign', () => {
    service.assignRider('o1').subscribe((res) => {
      expect(res.id).toBe('o1');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/orders/o1/assign-rider');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush(envelope(apiOrder));
  });

  it('should POST /api/orders/:id/assign-rider with riderId for manual assign', () => {
    service.assignRider('o1', 'rider1').subscribe();

    const req = httpMock.expectOne('http://localhost:5001/api/orders/o1/assign-rider');
    expect(req.request.body).toEqual({ riderId: 'rider1' });
    req.flush(envelope(apiOrder));
  });

  it('should keep business-validation envelope on createOrder (success: false)', () => {
    const failEnvelope = {
      success: false,
      message: 'Minimum order amount is 80',
      data: null,
      errors: null,
    };

    service.createOrder({ restaurantId: 'r1', items: [{ menuItemId: 'm1', quantity: 1 }] })
      .subscribe((res: any) => {
        expect(res.success).toBe(false);
        expect(res.message).toBe('Minimum order amount is 80');
      });

    const req = httpMock.expectOne('http://localhost:5001/api/orders');
    req.flush(failEnvelope);
  });
});

describe('normalizeOrderStatus', () => {
  it('maps PascalCase backend statuses to kebab-case', () => {
    expect(normalizeOrderStatus('Pending')).toBe('pending');
    expect(normalizeOrderStatus('AssignedToRider')).toBe('assignedToRider');
    expect(normalizeOrderStatus('OutForDelivery')).toBe('outForDelivery');
    expect(normalizeOrderStatus('Cancelled')).toBe('cancelled');
  });

  it('falls back to lowercase for unknown values', () => {
    expect(normalizeOrderStatus('Something')).toBe('something');
  });
});

describe('normalizeOrder', () => {
  it('fills item name from menuItemName and customerNote from notes', () => {
    const order = normalizeOrder({
      ...apiOrder,
      status: 'Delivered',
      notes: 'Por favor, llamar al llegar',
      items: [{ menuItemId: 'm1', menuItemName: 'Tacos al Pastor', quantity: 1, unitPrice: 89 }],
    });

    expect(order.status).toBe('delivered');
    expect(order.customerNote).toBe('Por favor, llamar al llegar');
    expect(order.items[0].name).toBe('Tacos al Pastor');
  });

  it('keeps name and customerNote when already present', () => {
    const order = normalizeOrder({
      ...apiOrder,
      status: 'Confirmed',
      name: undefined,
      customerNote: 'Nota directa',
      items: [{ menuItemId: 'm1', name: 'Tacos', quantity: 1, unitPrice: 89 }],
    });

    expect(order.customerNote).toBe('Nota directa');
    expect(order.items[0].name).toBe('Tacos');
  });

  it('returns non-order payloads untouched', () => {
    const err = { success: false, message: 'nope', data: null };
    expect(normalizeOrder(err)).toBe(err);
    expect(normalizeOrder(null)).toBeNull();
  });
});
