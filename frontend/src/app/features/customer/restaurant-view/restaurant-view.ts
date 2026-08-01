import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { RestaurantService, Restaurant, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService } from '../../../core/services/order.service';

export interface CartItem {
  menuItem: MenuItem;
  quantity: number;
  notes: string;
}

@Component({
  selector: 'app-restaurant-view',
  imports: [RouterLink],
  templateUrl: './restaurant-view.html',
  styleUrl: './restaurant-view.css',
})
export class RestaurantView implements OnInit {
  restaurant: Restaurant | null = null;
  menuCategories: MenuCategory[] = [];
  selectedCategoryId: string | null = null;
  loading = true;

  cart: CartItem[] = [];
  showCart = false;
  ordering = false;
  orderSuccess = false;
  orderError = '';

  constructor(
    private route: ActivatedRoute,
    private restaurantService: RestaurantService,
    protected auth: AuthService,
    private orderService: OrderService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.restaurantService.getBySlug(slug).subscribe({
        next: (restaurant) => {
          this.restaurant = restaurant;
          this.loadMenu(restaurant.id);
          this.applyTheme(restaurant.themeConfig);
        },
        error: () => (this.loading = false),
      });
    }
  }

  private loadMenu(restaurantId: string): void {
    this.restaurantService.getMenu(restaurantId).subscribe({
      next: (categories) => {
        this.menuCategories = categories;
        if (categories.length > 0) {
          this.selectedCategoryId = categories[0].id;
        }
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  private applyTheme(config: any): void {
    if (!config) return;
    const root = document.documentElement;
    if (config.primaryColor) root.style.setProperty('--restaurant-primary', config.primaryColor);
    if (config.secondaryColor) root.style.setProperty('--restaurant-secondary', config.secondaryColor);
    if (config.accentColor) root.style.setProperty('--restaurant-accent', config.accentColor);
    if (config.backgroundColor) root.style.setProperty('--restaurant-bg', config.backgroundColor);
    if (config.textColor) root.style.setProperty('--restaurant-text', config.textColor);
    if (config.fontFamily) root.style.setProperty('--restaurant-font', config.fontFamily);
  }

  get selectedCategory(): MenuCategory | undefined {
    return this.menuCategories.find(c => c.id === this.selectedCategoryId);
  }

  get availableCategories(): MenuCategory[] {
    return this.menuCategories.filter(c => c.items?.some(i => i.isAvailable));
  }

  categoryItemCount(category: MenuCategory): number {
    return category.items?.filter(i => i.isAvailable).length || 0;
  }

  // ── Cart methods ──

  addToCart(item: MenuItem): void {
    const existing = this.cart.find(c => c.menuItem.id === item.id);
    if (existing) {
      existing.quantity++;
    } else {
      this.cart.push({ menuItem: item, quantity: 1, notes: '' });
    }
  }

  removeFromCart(itemId: string): void {
    this.cart = this.cart.filter(c => c.menuItem.id !== itemId);
  }

  incrementQuantity(itemId: string): void {
    const item = this.cart.find(c => c.menuItem.id === itemId);
    if (item) item.quantity++;
  }

  decrementQuantity(itemId: string): void {
    const item = this.cart.find(c => c.menuItem.id === itemId);
    if (item) {
      if (item.quantity <= 1) {
        this.removeFromCart(itemId);
      } else {
        item.quantity--;
      }
    }
  }

  getCartQuantity(itemId: string): number {
    const item = this.cart.find(c => c.menuItem.id === itemId);
    return item ? item.quantity : 0;
  }

  get cartTotal(): number {
    return this.cart.reduce((sum, item) => sum + (item.menuItem.price * item.quantity), 0);
  }

  get cartItemCount(): number {
    return this.cart.reduce((sum, item) => sum + item.quantity, 0);
  }

  toggleCart(): void {
    this.showCart = !this.showCart;
  }

  placeOrder(): void {
    if (!this.restaurant || this.cart.length === 0) return;
    this.ordering = true;
    this.orderError = '';

    const items = this.cart.map(c => ({
      menuItemId: c.menuItem.id,
      quantity: c.quantity,
    }));

    this.orderService.createOrder({
      restaurantId: this.restaurant.id,
      items,
    }).subscribe({
      next: () => {
        this.orderSuccess = true;
        this.cart = [];
        this.showCart = false;
        this.ordering = false;
        setTimeout(() => (this.orderSuccess = false), 5000);
      },
      error: (err) => {
        this.orderError = 'Error al crear el pedido. Intenta de nuevo.';
        this.ordering = false;
      },
    });
  }
}