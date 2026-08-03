import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RestaurantService, Restaurant, ThemeConfig } from '../../../core/services/restaurant.service';
import { readableApiError } from '../../../core/utils/restaurant-onboarding';

@Component({
  selector: 'app-storefront-editor',
  imports: [FormsModule],
  templateUrl: './storefront-editor.html',
  styleUrl: './storefront-editor.css',
})
export class StorefrontEditor implements OnInit {
  restaurant: Restaurant | null = null;
  loading = true;
  saving = false;
  saved = signal(false);
  error = '';

  theme: ThemeConfig = {
    primaryColor: '#d4852a',
    secondaryColor: '#f9eddb',
    accentColor: '#e8bb7d',
    backgroundColor: '#faf9f7',
    textColor: '#1a1a2e',
    fontFamily: 'Playfair Display',
    logoUrl: '',
    coverImageUrl: '',
  };

  name = '';
  description = '';
  slug = '';

  constructor(
    private auth: AuthService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    const id = this.auth.currentUser()?.id;
    if (id) {
      this.restaurantService.getById(id).subscribe({
        next: (r) => {
          this.restaurant = r;
          this.name = r.name;
          this.description = r.description;
          this.slug = r.slug;
          if (r.themeConfig) {
            this.theme = { ...this.theme, ...r.themeConfig };
          }
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.error = 'No se pudo cargar la tienda';
        },
      });
    }
  }

  save(): void {
    this.saving = true;
    this.error = '';
    this.saved.set(false);

    const data = {
      name: this.name,
      description: this.description,
      slug: this.slug,
      themeConfig: this.theme,
    };

    if (this.restaurant) {
      this.restaurantService.update(this.restaurant.id, data).subscribe({
        next: () => {
          this.saving = false;
          this.saved.set(true);
          setTimeout(() => this.saved.set(false), 3000);
        },
        error: (e) => {
          this.saving = false;
          this.error = readableApiError(e, 'Error al guardar');
        },
      });
    }
  }

  previewStyle(): Partial<CSSStyleDeclaration> {
    return {
      backgroundColor: this.theme.primaryColor,
      fontFamily: `${this.theme.fontFamily}, serif`,
    };
  }

  cardPreviewStyle(): Partial<CSSStyleDeclaration> {
    return {
      backgroundColor: this.theme.backgroundColor,
      color: this.theme.textColor,
      borderColor: this.theme.primaryColor,
    };
  }
}
