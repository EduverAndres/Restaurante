import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiErrorEnvelope } from './api-response';

export interface Review {
  id: string;
  restaurantId: string;
  customerId: string;
  customerName: string;
  orderId: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface CreateReviewRequest {
  orderId: string;
  rating: number;
  comment?: string;
}

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly apiUrl = `${environment.apiUrl}/restaurants`;

  constructor(private http: HttpClient) {}

  createReview(restaurantId: string, data: CreateReviewRequest): Observable<Review | ApiErrorEnvelope> {
    return this.http.post<any>(`${this.apiUrl}/${restaurantId}/reviews`, data).pipe(
      map((res: any) => res.data || res)
    );
  }
}
