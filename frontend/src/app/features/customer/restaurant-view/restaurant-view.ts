import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RestaurantService, Restaurant, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { CartDrawer } from '../../../core/components/cart-drawer/cart-drawer';

interface ItemFormState {
  item: MenuItem;
  quantity: number;
  notes: string;
}

@Component({
  selector: 'app-restaurant-view',
  imports: [RouterLink, CartDrawer, FormsModule],
  templateUrl: './restaurant-view.html',
  styleUrl: './restaurant-view.css',
})
export class RestaurantView implements OnInit {
  restaurant: Restaurant | null = null;
  menuCategories: MenuCategory[] = [];
  selectedCategoryId: string | null = null;
  loading = true;

  showCart = false;
  itemForm: ItemFormState | null = null;

  constructor(
    private route: ActivatedRoute,
    private restaurantService: RestaurantService,
    protected auth: AuthService,
    protected cart: CartService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.restaurantService.getBySlug(slug).subscribe({
        next: (restaurant) => {
          this.restaurant = restaurant;
          this.cart.setRestaurant(restaurant);
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

  // ── Cart (global CartService) ──

  getCartQuantity(itemId: string): number {
    return this.cart.items().find(i => i.menuItem.id === itemId)?.quantity ?? 0;
  }

  openItemForm(item: MenuItem): void {
    this.itemForm = { item, quantity: 1, notes: '' };
  }

  incrementFormQty(): void {
    if (this.itemForm) this.itemForm.quantity = Math.min(99, this.itemForm.quantity + 1);
  }

  decrementFormQty(): void {
    if (this.itemForm) this.itemForm.quantity = Math.max(1, this.itemForm.quantity - 1);
  }

  confirmAddItem(): void {
    if (!this.itemForm) return;
    this.cart.addItem(this.itemForm.item, this.itemForm.quantity, this.itemForm.notes.trim() || undefined);
    this.itemForm = null;
  }

  cancelItemForm(): void {
    this.itemForm = null;
  }

  incrementQuantity(itemId: string): void {
    this.cart.updateQuantity(itemId, this.getCartQuantity(itemId) + 1);
  }

  decrementQuantity(itemId: string): void {
    this.cart.updateQuantity(itemId, this.getCartQuantity(itemId) - 1);
  }

  toggleCart(): void {
    this.showCart = !this.showCart;
  }
}
