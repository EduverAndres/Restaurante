import { Component, OnInit, ElementRef, ViewChild, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { AiService, Message } from '../../../core/services/ai.service';
import { parseMessageContent } from '../../../core/services/ai-response-validator';
import { OrderService, CreateOrderRequest } from '../../../core/services/order.service';
import { RestaurantService } from '../../../core/services/restaurant.service';

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
  messages: Message[] = [];
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
        this.messages = conv.messages || [];
        if (this.messages.length === 0) {
          this.messages.push({
            id: 'welcome',
            conversationId: conv.id,
            role: 'assistant',
            content: `¡Hola! Soy el asistente de ${this.restaurantName}. ¿Qué se te antoja hoy? Puedes decirme qué tipo de comida buscas, algún platillo en específico, o dejarme recomendarte algo.`,
            createdAt: new Date().toISOString(),
          });
        }
        this.loading.set(false);
        this.scrollToBottom();
      },
      error: (err) => {
        this.error = 'Error al iniciar la conversación';
        this.loading.set(false);
      },
    });
  }

  sendMessage(): void {
    const text = this.newMessage.trim();
    if (!text || !this.conversationId || this.loading()) return;

    const userMsg: Message = {
      id: `temp-${Date.now()}`,
      conversationId: this.conversationId,
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
        this.messages.push(reply);
        this.loading.set(false);
        this.scrollToBottom();

        const parsed = parseMessageContent(reply.content);
        if (parsed) {
          this.orderSummary = parsed;
        }
      },
      error: () => {
        this.messages.push({
          id: `err-${Date.now()}`,
          conversationId: this.conversationId!,
          role: 'assistant',
          content: 'Lo siento, tuve un problema. ¿Puedes intentarlo de nuevo?',
          createdAt: new Date().toISOString(),
        });
        this.loading.set(false);
        this.scrollToBottom();
      },
    });
  }

  confirmOrder(): void {
    if (!this.orderSummary?.items) return;

    const req: CreateOrderRequest = {
      restaurantId: this.restaurantId,
      items: this.orderSummary.items.map((i: any) => ({
        menuItemId: i.id,
        quantity: i.quantity || 1,
      })),
      customerNote: this.orderSummary.note || '',
    };

    this.loading.set(true);
    this.orderService.createOrder(req).subscribe({
      next: (order) => {
        this.orderPlaced = true;
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

  private scrollToBottom(): void {
    setTimeout(() => {
      try {
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      } catch {}
    }, 100);
  }
}
