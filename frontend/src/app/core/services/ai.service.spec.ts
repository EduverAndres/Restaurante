import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AiService, Conversation, Message } from './ai.service';

describe('AiService', () => {
  let service: AiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
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
    const mockResponse: Conversation = {
      id: 'c1',
      customerId: 'u1',
      restaurantId: 'r1',
      messages: [],
      status: 'active',
      createdAt: '2026-01-01T00:00:00Z',
    };

    service.startConversation('r1').subscribe((res) => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/ai/conversation/start');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ restaurantId: 'r1' });
    req.flush(mockResponse);
  });

  it('should POST to /api/ai/conversation/:id/message on sendMessage', () => {
    const mockResponse: Message = {
      id: 'm1',
      conversationId: 'c1',
      role: 'user',
      content: 'Hola',
      createdAt: '2026-01-01T00:00:00Z',
    };

    service.sendMessage('c1', 'Hola').subscribe((res) => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/ai/conversation/c1/message');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ content: 'Hola' });
    req.flush(mockResponse);
  });

  it('should GET /api/ai/conversation/:id on getConversation', () => {
    const mockResponse: Conversation = {
      id: 'c1',
      customerId: 'u1',
      restaurantId: 'r1',
      messages: [],
      status: 'active',
      createdAt: '2026-01-01T00:00:00Z',
    };

    service.getConversation('c1').subscribe((res) => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/ai/conversation/c1');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should GET /api/ai/conversations on getAllConversations', () => {
    const mockResponse: Conversation[] = [
      {
        id: 'c1',
        customerId: 'u1',
        restaurantId: 'r1',
        messages: [],
        status: 'active',
        createdAt: '2026-01-01T00:00:00Z',
      },
    ];

    service.getAllConversations().subscribe((res) => {
      expect(res).toHaveLength(1);
      expect(res[0].id).toBe('c1');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/ai/conversations');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should handle HTTP error gracefully', () => {
    service.startConversation('r1').subscribe({
      next: () => { throw new Error('should not succeed'); },
      error: (err) => {
        expect(err.status).toBe(500);
      },
    });

    const req = httpMock.expectOne('http://localhost:5000/api/ai/conversation/start');
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
  });
});
