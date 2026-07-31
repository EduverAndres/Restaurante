import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Conversation {
  id: string;
  customerId: string;
  restaurantId: string;
  messages: Message[];
  status: 'active' | 'completed' | 'cancelled';
  createdAt: string;
}

export interface Message {
  id: string;
  conversationId: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/ai`;

  constructor(private http: HttpClient) {}

  startConversation(restaurantId: string): Observable<Conversation> {
    return this.http.post<Conversation>(`${this.apiUrl}/conversation/start`, { restaurantId });
  }

  sendMessage(conversationId: string, message: string): Observable<Message> {
    return this.http.post<Message>(`${this.apiUrl}/conversation/${conversationId}/message`, { content: message });
  }

  getConversation(conversationId: string): Observable<Conversation> {
    return this.http.get<Conversation>(`${this.apiUrl}/conversation/${conversationId}`);
  }

  getAllConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.apiUrl}/conversations`);
  }
}
