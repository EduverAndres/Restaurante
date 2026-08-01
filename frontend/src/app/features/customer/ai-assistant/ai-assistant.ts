import { Component, OnInit, ElementRef, ViewChild, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { AiService } from '../../../core/services/ai.service';
import { parseMessageContent } from '../../../core/services/ai-response-validator';
import { OrderService, CreateOrderRequest } from '../../../core/services/order.service';
import { RestaurantService } from '../../../core/services/restaurant.service';

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

@Component({
  selector: 'app-ai-assistant',
  imports: [FormsModule, DatePipe],
  templateUrl: './ai-assistant.html',
  styleUrl: './ai-assistant.css',
})
export class AiAssistant implements OnInit {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  conversationId: string | null = null;
  restaurantId: string = '';
  restaurantName: string = '';
  messages: ChatMessage[] = [];
  newMessage = '';
  loading = signal(false);
  orderSummary: any = null;
  orderPlaced = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private aiService: AiService,
    private orderService: OrderService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.restaurantService.getBySlug(slug).subscribe({
        next: (r) => {
          this.restaurantId = r.id;
          this.restaurantName = r.name;
          this.startConversation();
        },
      });
    }
  }

  private startConversation(): void {
    this.loading.set(true);
    this.aiService.startConversation(this.restaurantId).subscribe({
      next: (conv) => {
        this.conversationId = conv.id;
        this.messages = this.parseHistory(conv.messages);
        if (this.messages.length === 0) {
          this.messages.push({
            id: 'welcome',
            role: 'assistant',
            content: `¡Hola! Soy el asistente de ${this.restaurantName}. ¿Qué se te antoja hoy? Puedes decirme qué tipo de comida buscas, algún platillo en específico, o dejarme recomendarte algo.`,
            createdAt: new Date().toISOString(),
          });
        }
        this.loading.set(false);
        this.scrollToBottom();
      },
      error: () => {
        this.error = 'Error al iniciar la conversación';
        this.loading.set(false);
      },
    });
  }

  sendMessage(): void {
    const text = this.newMessage.trim();
    if (!text || !this.conversationId || this.loading()) return;

    const userMsg: ChatMessage = {
      id: `temp-${Date.now()}`,
      role: 'user',
      content: text,
      createdAt: new Date().toISOString(),
    };

    this.messages.push(userMsg);
    this.newMessage = '';
    this.loading.set(true);
    this.scrollToBottom();

    this.aiService.sendMessage(this.conversationId, text).subscribe({
      next: (reply) => {
        this.handleAssistantResponse(reply.summary);
        this.loading.set(false);
        this.scrollToBottom();
      },
      error: () => {
        this.messages.push({
          id: `err-${Date.now()}`,
          role: 'assistant',
          content: 'Lo siento, tuve un problema. ¿Puedes intentarlo de nuevo?',
          createdAt: new Date().toISOString(),
        });
        this.loading.set(false);
        this.scrollToBottom();
      },
    });
  }

  private handleAssistantResponse(summary: string): void {
    const parsed = parseMessageContent(summary);
    if (parsed) {
      this.orderSummary = parsed;
      this.messages.push({
        id: `ai-${Date.now()}`,
        role: 'assistant',
        content: parsed.summary,
        createdAt: new Date().toISOString(),
      });
    } else {
      this.messages.push({
        id: `ai-${Date.now()}`,
        role: 'assistant',
        content: summary,
        createdAt: new Date().toISOString(),
      });
    }
  }

  confirmOrder(): void {
    if (!this.orderSummary?.items) return;

    const req: CreateOrderRequest = {
      restaurantId: this.restaurantId,
      items: this.orderSummary.items.map((i: any) => ({
        menuItemId: i.menuItemId,
        quantity: i.quantity,
      })),
    };

    this.loading.set(true);
    this.orderService.createOrder(req).subscribe({
      next: () => {
        this.orderPlaced = true;
        this.orderSummary = null;
        this.loading.set(false);
      },
      error: () => {
        this.error = 'Error al crear el pedido';
        this.loading.set(false);
      },
    });
  }

  cancelOrder(): void {
    this.orderSummary = null;
  }

  viewOrder(): void {
    this.router.navigate(['/customer/orders']);
  }

  private parseHistory(raw: string): ChatMessage[] {
    if (!raw) return [];
    const parts = raw.split(/\n(?=(?:User|AI): )/);
    return parts
      .map((part) => part.match(/^(User|AI): ([\s\S]*)$/))
      .filter((m): m is RegExpMatchArray => m !== null)
      .map((m, i) => ({
        id: `hist-${i}`,
        role: m[1] === 'User' ? 'user' : 'assistant',
        content: m[2],
        createdAt: new Date().toISOString(),
      }));
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      try {
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      } catch {}
    }, 100);
  }
}
