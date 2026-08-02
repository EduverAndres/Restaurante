import { Injectable, computed, signal } from '@angular/core';
import { MenuItem, Restaurant } from './restaurant.service';

export interface CartItem {
  menuItem: MenuItem;
  quantity: number;
  notes?: string;
}

export interface CartRestaurant {
  id: string;
  name: string;
  slug: string;
  deliveryFee: number;
  minOrderAmount: number;
}

const CART_KEY = 'restaurante_cart';
const RESTAURANT_KEY = 'restaurante_cart_restaurant';
const MAX_QUANTITY = 99;

@Injectable({ providedIn: 'root' })
export class CartService {
  items = signal<CartItem[]>([]);
  restaurant = signal<CartRestaurant | null>(null);
  couponCode = signal<string | null>(null);

  count = computed(() => this.items().reduce((sum, i) => sum + i.quantity, 0));
  subtotal = computed(() => this.items().reduce((sum, i) => sum + i.menuItem.price * i.quantity, 0));
  restaurantId = computed(() => this.restaurant()?.id ?? null);

  constructor() {
    this.hydrate();
  }

  setRestaurant(restaurant: Restaurant): void {
    // Cart is per-restaurant: switching restaurants discards the current items
    // (documented decision — dev simple; a confirm dialog is future work).
    if (restaurant.id !== this.restaurantId() && this.items().length > 0) {
      this.clear();
    }
    this.restaurant.set({
      id: restaurant.id,
      name: restaurant.name,
      slug: restaurant.slug,
      deliveryFee: restaurant.deliveryFee ?? 0,
      minOrderAmount: restaurant.minOrderAmount ?? 0,
    });
    this.persistRestaurant();
  }

  addItem(menuItem: MenuItem, quantity = 1, notes?: string): void {
    const qty = Math.min(MAX_QUANTITY, Math.max(1, Math.floor(quantity)));
    if (menuItem.restaurantId !== this.restaurantId() && this.items().length > 0) {
      this.clear();
    }
    const existing = this.items().find(i => i.menuItem.id === menuItem.id);
    if (existing) {
      this.items.update(items =>
        items.map(i =>
          i.menuItem.id === menuItem.id
            ? { ...i, quantity: Math.min(MAX_QUANTITY, i.quantity + qty), notes: notes ?? i.notes }
            : i,
        ),
      );
    } else {
      this.items.update(items => [...items, { menuItem, quantity: qty, notes }]);
    }
    this.persistItems();
  }

  updateQuantity(menuItemId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(menuItemId);
      return;
    }
    const qty = Math.min(MAX_QUANTITY, Math.floor(quantity));
    this.items.update(items => items.map(i => (i.menuItem.id === menuItemId ? { ...i, quantity: qty } : i)));
    this.persistItems();
  }

  removeItem(menuItemId: string): void {
    this.items.update(items => items.filter(i => i.menuItem.id !== menuItemId));
    this.persistItems();
  }

  clear(): void {
    this.items.set([]);
    this.couponCode.set(null);
    this.persistItems();
  }

  setCouponCode(code: string | null): void {
    this.couponCode.set(code && code.trim() ? code.trim() : null);
  }

  private hydrate(): void {
    try {
      const rawItems = localStorage.getItem(CART_KEY);
      if (rawItems) this.items.set(JSON.parse(rawItems) as CartItem[]);
      const rawRestaurant = localStorage.getItem(RESTAURANT_KEY);
      if (rawRestaurant) this.restaurant.set(JSON.parse(rawRestaurant) as CartRestaurant);
    } catch {
      // Corrupted storage: start fresh
    }
  }

  private persistItems(): void {
    try {
      localStorage.setItem(CART_KEY, JSON.stringify(this.items()));
    } catch {
      // Storage unavailable: keep in-memory state only
    }
  }

  private persistRestaurant(): void {
    try {
      localStorage.setItem(RESTAURANT_KEY, JSON.stringify(this.restaurant()));
    } catch {
      // Storage unavailable: keep in-memory state only
    }
  }
}
