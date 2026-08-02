import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ReviewService } from './review.service';
import { apiResponseInterceptor } from '../interceptors/api-response.interceptor';

const reviewData = {
  id: 'rv1',
  restaurantId: 'r1',
  customerId: 'c1',
  customerName: 'María García',
  orderId: 'o1',
  rating: 5,
  comment: 'Excelente',
  createdAt: '2026-08-01T20:00:00Z',
};

describe('ReviewService', () => {
  let service: ReviewService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiResponseInterceptor])),
        provideHttpClientTesting(),
        ReviewService,
      ],
    });

    service = TestBed.inject(ReviewService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST /api/restaurants/:id/reviews with orderId, rating and comment', () => {
    service.createReview('r1', { orderId: 'o1', rating: 5, comment: 'Excelente' })
      .subscribe((res) => {
        expect(res).toEqual(reviewData);
        expect(res.rating).toBe(5);
      });

    const req = httpMock.expectOne('http://localhost:5001/api/restaurants/r1/reviews');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ orderId: 'o1', rating: 5, comment: 'Excelente' });
    req.flush({ success: true, message: 'ok', data: reviewData, errors: null });
  });

  it('should send comment-less review when omitted', () => {
    service.createReview('r1', { orderId: 'o1', rating: 4 }).subscribe();

    const req = httpMock.expectOne('http://localhost:5001/api/restaurants/r1/reviews');
    expect(req.request.body).toEqual({ orderId: 'o1', rating: 4 });
    req.flush({ success: true, message: 'ok', data: reviewData, errors: null });
  });

  it('should keep business-validation envelope on failure', () => {
    const failEnvelope = {
      success: false,
      message: 'Order must be delivered before reviewing',
      data: null,
      errors: null,
    };

    service.createReview('r1', { orderId: 'o1', rating: 5 }).subscribe((res: any) => {
      expect(res.success).toBe(false);
      expect(res.message).toBe('Order must be delivered before reviewing');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/restaurants/r1/reviews');
    req.flush(failEnvelope);
  });
});
