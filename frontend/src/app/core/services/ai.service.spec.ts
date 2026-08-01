import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AiService } from './ai.service';
import { apiResponseInterceptor } from '../interceptors/api-response.interceptor';

const conversationData = {
  id: 'c1',
  messages: 'User: Hola\nAI: ¡Hola! ¿Qué se te antoja?',
  summary: '¡Hola! ¿Qué se te antoja?',
};

const apiResponse = {
  success: true,
  message: 'ok',
  data: conversationData,
  errors: null,
};

describe('AiService', () => {
  let service: AiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiResponseInterceptor])),
        provideHttpClientTesting(),
        AiService,
      ],
    });

    service = TestBed.inject(AiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST to /api/ai/conversation/start on startConversation', () => {
    service.startConversation('r1').subscribe((res) => {
      expect(res).toEqual(conversationData);
      expect(res.summary).toBe(conversationData.summary);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversation/start');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ restaurantId: 'r1' });
    req.flush(apiResponse);
  });

  it('should include initialMessage in startConversation body when provided', () => {
    service.startConversation('r1', 'Quiero una pizza').subscribe();

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversation/start');
    expect(req.request.body).toEqual({ restaurantId: 'r1', initialMessage: 'Quiero una pizza' });
    req.flush(apiResponse);
  });

  it('should POST to /api/ai/conversation/:id/message on sendMessage', () => {
    service.sendMessage('c1', 'Hola').subscribe((res) => {
      expect(res).toEqual(conversationData);
      expect(res.summary).toBe(conversationData.summary);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversation/c1/message');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ content: 'Hola' });
    req.flush(apiResponse);
  });

  it('should GET /api/ai/conversation/:id on getConversation', () => {
    service.getConversation('c1').subscribe((res) => {
      expect(res.id).toBe('c1');
      expect(res.messages).toBe(conversationData.messages);
    });

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversation/c1');
    expect(req.request.method).toBe('GET');
    req.flush(apiResponse);
  });

  it('should GET /api/ai/conversations on getAllConversations', () => {
    service.getAllConversations().subscribe((res) => {
      expect(res).toHaveLength(1);
      expect(res[0].id).toBe('c1');
    });

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversations');
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, message: 'ok', data: [conversationData], errors: null });
  });

  it('should handle HTTP error gracefully', () => {
    service.startConversation('r1').subscribe({
      next: () => { throw new Error('should not succeed'); },
      error: (err) => {
        expect(err.status).toBe(500);
      },
    });

    const req = httpMock.expectOne('http://localhost:5001/api/ai/conversation/start');
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
  });
});
