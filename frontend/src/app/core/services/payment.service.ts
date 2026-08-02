import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiErrorEnvelope } from './api-response';

export type PaymentMethod = 'CARD' | 'CASH';

export interface Payment {
  id: string;
  orderId: string;
  amount: number;
  method: string;
  status: string;
  transactionId?: string;
  reference?: string;
  createdAt: string;
}

export interface ProcessPaymentRequest {
  orderId: string;
  method: PaymentMethod;
  cardToken?: string;
  acceptanceToken?: string;
  customerEmail?: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  checkout(data: ProcessPaymentRequest): Observable<Payment | ApiErrorEnvelope> {
    return this.http.post<any>(`${this.apiUrl}/checkout`, data).pipe(
      map((res: any) => res.data || res)
    );
  }
}
