import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AIConversation {
  id: string;
  messages: string;
  summary: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/ai`;

  constructor(private http: HttpClient) {}

  startConversation(restaurantId: string, initialMessage?: string): Observable<AIConversation> {
    const body = initialMessage ? { restaurantId, initialMessage } : { restaurantId };
    return this.http.post<AIConversation>(`${this.apiUrl}/conversation/start`, body);
  }

  sendMessage(conversationId: string, message: string): Observable<AIConversation> {
    return this.http.post<AIConversation>(`${this.apiUrl}/conversation/${conversationId}/message`, { content: message });
  }

  getConversation(conversationId: string): Observable<AIConversation> {
    return this.http.get<AIConversation>(`${this.apiUrl}/conversation/${conversationId}`);
  }

  getAllConversations(): Observable<AIConversation[]> {
    return this.http.get<AIConversation[]>(`${this.apiUrl}/conversations`);
  }
}
