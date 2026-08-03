import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiErrorEnvelope } from './api-response';

export interface AIConversation {
  id: string;
  messages: string;
  summary: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/ai`;

  constructor(private http: HttpClient) {}

  /**
   * Business validation failures arrive as HTTP 200 with { success: false, message, data: null }
   * (see apiResponseInterceptor). Surface them as thrown errors so callers can read `message`.
   */
  private unwrap<T>(value: any): T {
    if (isApiErrorEnvelope(value)) throw value;
    return value as T;
  }

  startConversation(restaurantId: string, initialMessage?: string): Observable<AIConversation> {
    const body = initialMessage ? { restaurantId, initialMessage } : { restaurantId };
    return this.http.post<any>(`${this.apiUrl}/conversation/start`, body).pipe(
      map((res: any) => this.unwrap<AIConversation>(res.data || res))
    );
  }

  sendMessage(conversationId: string, message: string): Observable<AIConversation> {
    return this.http.post<any>(`${this.apiUrl}/conversation/${conversationId}/message`, { content: message }).pipe(
      map((res: any) => this.unwrap<AIConversation>(res.data || res))
    );
  }

  getConversation(conversationId: string): Observable<AIConversation> {
    return this.http.get<any>(`${this.apiUrl}/conversation/${conversationId}`).pipe(
      map((res: any) => this.unwrap<AIConversation>(res.data || res))
    );
  }

  getAllConversations(): Observable<AIConversation[]> {
    return this.http.get<any>(`${this.apiUrl}/conversations`).pipe(
      map((res: any) => {
        const list = this.unwrap<any>(res.data || res);
        return Array.isArray(list) ? list : [];
      })
    );
  }
}
