import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PaymentService } from './payment.service';
import { apiResponseInterceptor } from '../interceptors/api-response.interceptor';

const paymentData = {
  id: 'p1',
  orderId: 'o1',
  amount: 303.3,
  method: 'CARD',
  status: 'Paid',
  transactionId: 'TXN-1234567890',
  reference: 'rest-o1',
  createdAt: '2026-08-01T18:05:00Z',
};

const envelope = (data: unknown) => ({ success: true, message: 'ok', data, errors: null });

describe('PaymentService', () => {
  let service: PaymentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiResponseInterceptor])),
        provideHttpClientTesting(),
        PaymentService,
      ],
    });

    service = TestBed.inject(PaymentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST /api/payments/checkout with CARD token data', () => {
    service.checkout({
      orderId: 'o1',
      method: 'CARD',
      cardToken: 'tok_test_123',
      acceptanceToken: 'acc_456',
      customerEmail: 'cliente@restaurante.app',
    }).subscribe((res) => {
      expect(res.status).toBe('Paid');
      expect(res.transactionId).toBe('TXN-1234567890');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/payments/checkout');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      orderId: 'o1',
      method: 'CARD',
      cardToken: 'tok_test_123',
      acceptanceToken: 'acc_456',
      customerEmail: 'cliente@restaurante.app',
    });
    req.flush(envelope(paymentData));
  });

  it('should POST checkout with CASH body (no card fields)', () => {
    service.checkout({ orderId: 'o1', method: 'CASH' }).subscribe((res) => {
      expect(res.method).toBe('CARD');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/payments/checkout');
    expect(req.request.body).toEqual({ orderId: 'o1', method: 'CASH' });
    req.flush(envelope(paymentData));
  });

  it('should keep business-validation envelope on failure', () => {
    const failEnvelope = {
      success: false,
      message: 'Order is not pending',
      data: null,
      errors: null,
    };

    service.checkout({ orderId: 'o1', method: 'CASH' }).subscribe((res: any) => {
      expect(res.success).toBe(false);
      expect(res.message).toBe('Order is not pending');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/payments/checkout');
    req.flush(failEnvelope);
  });
});
