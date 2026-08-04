import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RestaurantService, Restaurant, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { CartDrawer } from '../../../core/components/cart-drawer/cart-drawer';
import { SAMPLE_RESTAURANTS } from '../../../core/data/sample-restaurants';

interface ItemFormState {
  item: MenuItem;
  quantity: number;
  notes: string;
}

function generateSampleMenu(restaurantId: string, restaurantName: string): MenuCategory[] {
  const categories: MenuCategory[] = [
    {
      id: `${restaurantId}-cat-1`,
      restaurantId,
      name: 'Entradas',
      description: 'Para comenzar',
      displayOrder: 1,
      items: [
        { id: `${restaurantId}-item-1`, restaurantId, categoryId: `${restaurantId}-cat-1`, name: 'Entrada de la casa', description: 'Especialidad del chef para compartir', price: 8.50, isAvailable: true, displayOrder: 1 },
        { id: `${restaurantId}-item-2`, restaurantId, categoryId: `${restaurantId}-cat-1`, name: 'Sopa del día', description: 'Preparada fresca cada mañana', price: 6.00, isAvailable: true, displayOrder: 2 },
      ],
    },
    {
      id: `${restaurantId}-cat-2`,
      restaurantId,
      name: 'Platos principales',
      description: 'Nuestros favoritos',
      displayOrder: 2,
      items: [
        { id: `${restaurantId}-item-3`, restaurantId, categoryId: `${restaurantId}-cat-2`, name: 'Plato estrella', description: `El plato más pedido de ${restaurantName}`, price: 15.90, isAvailable: true, displayOrder: 1 },
        { id: `${restaurantId}-item-4`, restaurantId, categoryId: `${restaurantId}-cat-2`, name: 'Especialidad de la casa', description: 'Receta única preparada al momento', price: 18.50, isAvailable: true, displayOrder: 2 },
        { id: `${restaurantId}-item-5`, restaurantId, categoryId: `${restaurantId}-cat-2`, name: 'Opción vegetariana', description: 'Fresca y saludable', price: 12.00, isAvailable: true, displayOrder: 3 },
      ],
    },
    {
      id: `${restaurantId}-cat-3`,
      restaurantId,
      name: 'Bebidas',
      description: 'Para acompañar',
      displayOrder: 3,
      items: [
        { id: `${restaurantId}-item-6`, restaurantId, categoryId: `${restaurantId}-cat-3`, name: 'Bebida mediana', description: 'Refresco o jugo natural', price: 3.50, isAvailable: true, displayOrder: 1 },
        { id: `${restaurantId}-item-7`, restaurantId, categoryId: `${restaurantId}-cat-3`, name: 'Agua mineral', description: '500 ml', price: 2.00, isAvailable: true, displayOrder: 2 },
      ],
    },
  ];
  return categories;
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
  error = false;
  usingSampleData = false;

  showCart = false;
  itemForm: ItemFormState | null = null;

  constructor(
    private route: ActivatedRoute,
    private restaurantService: RestaurantService,
    protected auth: AuthService,
    protected cart: CartService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.loading = false;
      this.error = true;
      return;
    }

    this.restaurantService.getBySlug(slug).subscribe({
      next: (restaurant) => {
        if (!restaurant || !restaurant.id || restaurant.id === 'undefined' || restaurant.id === 'null') {
          this.trySampleFallback(slug);
          return;
        }

        this.restaurant = restaurant;
        this.cart.setRestaurant(restaurant);
        this.loadMenu(restaurant.id);
        this.applyTheme(restaurant.themeConfig);
      },
      error: () => this.trySampleFallback(slug),
    });
  }

  /** When the API fails (backend down or restaurant not found), try sample data. */
  private trySampleFallback(slug: string): void {
    const sample = SAMPLE_RESTAURANTS.find(r => r.slug === slug);
    if (sample) {
      this.restaurant = sample;
      this.usingSampleData = true;
      this.cart.setRestaurant(sample);
      this.menuCategories = generateSampleMenu(sample.id, sample.name);
      if (this.menuCategories.length > 0) {
        this.selectedCategoryId = this.menuCategories[0].id;
      }
      this.applyTheme(sample.themeConfig);
      this.loading = false;
    } else {
      this.error = true;
      this.loading = false;
    }
  }

  private loadMenu(restaurantId: string | null | undefined): void {
    if (!restaurantId || restaurantId === 'undefined' || restaurantId === 'null') {
      this.loading = false;
      return;
    }

    this.restaurantService.getMenu(restaurantId).subscribe({
      next: (categories) => {
        this.menuCategories = categories;
        if (categories.length > 0) {
          this.selectedCategoryId = categories[0].id;
        }
        this.loading = false;
      },
      error: () => {
        // If the menu API fails, generate a sample menu so the user can still interact.
        if (this.restaurant) {
          this.menuCategories = generateSampleMenu(this.restaurant.id, this.restaurant.name);
          if (this.menuCategories.length > 0) {
            this.selectedCategoryId = this.menuCategories[0].id;
          }
          this.usingSampleData = true;
        }
        this.loading = false;
      },
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

  scrollToMenu(): void {
    document.getElementById('menu-section')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
