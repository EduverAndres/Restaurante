import { Component, effect, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { RestaurantService } from '../../services/restaurant.service';
import { AuthService } from '../../services/auth.service';
import { isApiErrorEnvelope } from '../../services/api-response';

@Component({
  selector: 'app-cart-drawer',
  imports: [FormsModule],
  templateUrl: './cart-drawer.html',
  styleUrl: './cart-drawer.css',
})
export class CartDrawer {
  open = input(false);
  closed = output<void>();

  couponInput = '';
  couponApplied = false;
  restaurantGone = false;
  verifyingRestaurant = false;
  private lastVerifiedId: string | null = null;

  constructor(
    protected cart: CartService,
    protected auth: AuthService,
    private router: Router,
    private restaurantService: RestaurantService,
  ) {
    effect(() => {
      if (this.open()) this.onOpened();
    });
  }

  private onOpened(): void {
    const restaurantId = this.cart.restaurantId();
    if (restaurantId && restaurantId !== this.lastVerifiedId && this.cart.count() > 0) {
      this.lastVerifiedId = restaurantId;
      this.verifyRestaurant(restaurantId);
    }
  }

  private verifyRestaurant(restaurantId: string): void {
    this.verifyingRestaurant = true;
    this.restaurantService.getById(restaurantId).subscribe({
      next: (res: any) => {
        this.verifyingRestaurant = false;
        this.restaurantGone = !!res && isApiErrorEnvelope(res);
      },
      error: () => {
        this.verifyingRestaurant = false;
        this.restaurantGone = true;
      },
    });
  }

  close(): void {
    this.closed.emit();
  }

  quantityOf(itemId: string): number {
    return this.cart.items().find(i => i.menuItem.id === itemId)?.quantity ?? 0;
  }

  increment(itemId: string): void {
    this.cart.updateQuantity(itemId, this.quantityOf(itemId) + 1);
  }

  decrement(itemId: string): void {
    this.cart.updateQuantity(itemId, this.quantityOf(itemId) - 1);
  }

  applyCoupon(): void {
    this.cart.setCouponCode(this.couponInput);
    this.couponApplied = !!this.cart.couponCode();
    this.couponInput = '';
  }

  removeCoupon(): void {
    this.cart.setCouponCode(null);
    this.couponApplied = false;
  }

  get canContinue(): boolean {
    if (this.cart.count() === 0) return false;
    const min = this.cart.restaurant()?.minOrderAmount ?? 0;
    return this.cart.subtotal() >= min;
  }

  get minOrderShortfall(): number {
    const min = this.cart.restaurant()?.minOrderAmount ?? 0;
    return Math.max(0, min - this.cart.subtotal());
  }

  get deliveryFee(): number {
    return this.cart.restaurant()?.deliveryFee ?? 0;
  }

  get estimatedTotal(): number {
    return this.cart.subtotal() + this.deliveryFee;
  }

  clearCart(): void {
    this.cart.clear();
    this.restaurantGone = false;
    this.couponApplied = false;
    this.couponInput = '';
  }

  goBrowse(): void {
    this.close();
    this.router.navigate(['/browse']);
  }

  continueToCheckout(): void {
    if (!this.canContinue) return;
    this.close();
    this.router.navigate(['/checkout']);
  }
}
