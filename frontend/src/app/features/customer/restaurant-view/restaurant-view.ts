import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { RestaurantService, Restaurant, MenuCategory } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';

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

  constructor(
    private route: ActivatedRoute,
    private restaurantService: RestaurantService,
    protected auth: AuthService,
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
}
