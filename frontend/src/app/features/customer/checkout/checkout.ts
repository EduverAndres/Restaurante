import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { OrderService, Order } from '../../../core/services/order.service';
import { PaymentService, PaymentMethod } from '../../../core/services/payment.service';
import { AddressService, CustomerAddress } from '../../../core/services/address.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { isApiErrorEnvelope } from '../../../core/services/api-response';

const DEMO_CARD_TOKEN = 'tok_test_demo';

@Component({
  selector: 'app-checkout',
  imports: [FormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout implements OnInit {
  addresses: CustomerAddress[] = [];
  addressesLoading = true;
  selectedAddressId: string | null = null;
  showAddressForm = false;
  addressSaving = false;
  addressError = '';
  newAddress = { label: '', address: '', latitude: '', longitude: '', isDefault: false };

  paymentMethod: PaymentMethod = 'CASH';
  card = { number: '', expiry: '', cvv: '' };
  customerNote = '';

  error = '';
  submitting = false;
  orderCreated: Order | null = null;
  couponApplied = false;

  constructor(
    protected cart: CartService,
    protected auth: AuthService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private addressService: AddressService,
    private toast: ToastService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    if (this.cart.count() === 0) {
      this.router.navigate(['/browse']);
      return;
    }
    this.loadAddresses();
  }

  // ── Order summary ──

  get deliveryFee(): number {
    return this.cart.restaurant()?.deliveryFee ?? 0;
  }

  get discountAmount(): number {
    return this.orderCreated?.discountAmount ?? 0;
  }

  get total(): number {
    return this.orderCreated ? this.orderCreated.total : this.cart.subtotal() + this.deliveryFee;
  }

  // ── Addresses ──

  private loadAddresses(): void {
    this.addressesLoading = true;
    this.addressService.getAddresses().subscribe({
      next: (res: any) => {
        this.addressesLoading = false;
        if (res && isApiErrorEnvelope(res)) {
          this.addressError = res.message;
          return;
        }
        this.addresses = res || [];
        const def = this.addresses.find(a => a.isDefault);
        this.selectedAddressId = def?.id ?? this.addresses[0]?.id ?? null;
        this.showAddressForm = this.addresses.length === 0;
      },
      error: () => {
        this.addressesLoading = false;
        this.showAddressForm = true;
      },
    });
  }

  toggleAddressForm(): void {
    this.showAddressForm = !this.showAddressForm;
    this.addressError = '';
  }

  selectAddress(id: string): void {
    this.selectedAddressId = id;
  }

  saveAddress(): void {
    if (!this.newAddress.label.trim() || !this.newAddress.address.trim()) {
      this.addressError = 'Completa el nombre y la dirección';
      return;
    }
    this.addressSaving = true;
    this.addressError = '';
    this.addressService.createAddress({
      label: this.newAddress.label.trim(),
      address: this.newAddress.address.trim(),
      latitude: this.newAddress.latitude ? Number(this.newAddress.latitude) : null,
      longitude: this.newAddress.longitude ? Number(this.newAddress.longitude) : null,
      isDefault: this.newAddress.isDefault,
    }).subscribe({
      next: (res: any) => {
        this.addressSaving = false;
        if (res && isApiErrorEnvelope(res)) {
          this.addressError = res.message;
          return;
        }
        this.addresses = [res, ...this.addresses];
        this.selectedAddressId = res.id;
        this.newAddress = { label: '', address: '', latitude: '', longitude: '', isDefault: false };
        this.showAddressForm = false;
      },
      error: (err) => {
        this.addressSaving = false;
        this.addressError = err.error?.message || 'No se pudo guardar la dirección';
      },
    });
  }

  // ── Order flow ──

  get canSubmit(): boolean {
    return this.selectedAddressId !== null && this.cart.count() > 0;
  }

  confirmOrder(): void {
    if (!this.canSubmit || this.submitting) return;
    this.error = '';

    if (this.orderCreated) {
      // Payment failed earlier: retry against the same order instead of duplicating it.
      this.pay(this.orderCreated);
      return;
    }

    const restaurantId = this.cart.restaurantId();
    if (!restaurantId) return;

    this.submitting = true;
    this.orderService.createOrder({
      restaurantId,
      items: this.cart.items().map(i => ({ menuItemId: i.menuItem.id, quantity: i.quantity, notes: i.notes })),
      notes: this.customerNote.trim() || undefined,
    }).subscribe({
      next: (res: any) => {
        if (res && isApiErrorEnvelope(res)) {
          this.submitting = false;
          this.error = res.message;
          return;
        }
        this.orderCreated = res;
        this.applyCouponIfNeeded(res);
      },
      error: (err) => {
        this.submitting = false;
        this.error = err.error?.message || 'No se pudo crear el pedido. Intenta de nuevo.';
      },
    });
  }

  private applyCouponIfNeeded(order: Order): void {
    const code = this.cart.couponCode();
    if (!code) {
      this.pay(order);
      return;
    }

    this.orderService.applyCoupon(order.id, code).subscribe({
      next: (res: any) => {
        if (res && isApiErrorEnvelope(res)) {
          this.toast.show(`Cupón no aplicado: ${res.message}`, 'error');
          this.pay(order);
          return;
        }
        this.couponApplied = true;
        this.orderCreated = res;
        this.pay(res);
      },
      error: () => {
        this.toast.show('No se pudo aplicar el cupón. El pedido continúa sin descuento.', 'info');
        this.pay(order);
      },
    });
  }

  private pay(order: Order): void {
    if (this.paymentMethod === 'CARD') {
      if (!this.card.number.trim() || !this.card.expiry.trim() || !this.card.cvv.trim()) {
        this.submitting = false;
        this.error = 'Completa los datos de la tarjeta (modo demo)';
        return;
      }
    }

    this.paymentService.checkout({
      orderId: order.id,
      method: this.paymentMethod,
      cardToken: this.paymentMethod === 'CARD' ? DEMO_CARD_TOKEN : undefined,
      customerEmail: this.auth.currentUser()?.email,
    }).subscribe({
      next: (res: any) => {
        if (res && isApiErrorEnvelope(res)) {
          this.submitting = false;
          this.error = res.message;
          return;
        }
        if (res.status === 'Failed') {
          this.submitting = false;
          this.error = 'El pago fue rechazado. El pedido quedó pendiente; puedes reintentar el pago.';
          return;
        }
        this.submitting = false;
        this.toast.show('¡Pedido confirmado y pagado!', 'success');
        this.cart.clear();
        this.router.navigate(['/customer/orders', order.id]);
      },
      error: (err) => {
        this.submitting = false;
        this.error = err.error?.message || 'El pago no pudo procesarse. Puedes reintentar.';
      },
    });
  }
}
