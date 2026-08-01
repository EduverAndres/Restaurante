import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-restaurant-profile',
  imports: [FormsModule],
  templateUrl: './restaurant-profile.html',
  styleUrl: './restaurant-profile.css',
})
export class RestaurantProfile implements OnInit {
  restaurant: Restaurant | null = null;
  loading = true;
  saving = false;
  saved = signal(false);
  error = '';
  successMessage = '';

  // Profile form
  name = '';
  description = '';
  slug = '';
  phone = '';
  address = '';
  logoUrl = '';
  coverImageUrl = '';
  openingHours = '';
  deliveryEnabled = true;
  minOrderAmount = 0;
  deliveryFee = 0;

  constructor(
    protected auth: AuthService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    this.loadRestaurant();
  }

  private loadRestaurant(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.restaurant = restaurants[0];
          this.populateForm();
        }
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  private populateForm(): void {
    if (!this.restaurant) return;
    this.name = this.restaurant.name;
    this.description = this.restaurant.description;
    this.slug = this.restaurant.slug;
    this.logoUrl = this.restaurant.logoUrl || '';
    this.coverImageUrl = this.restaurant.coverImageUrl || '';
  }

  save(): void {
    if (!this.restaurant) return;
    this.saving = true;
    this.error = '';

    const data: Partial<Restaurant> = {
      name: this.name,
      description: this.description,
      slug: this.slug,
      logoUrl: this.logoUrl,
      coverImageUrl: this.coverImageUrl,
    };

    this.restaurantService.update(this.restaurant.id, data).subscribe({
      next: (updated) => {
        this.restaurant = updated;
        this.saving = false;
        this.saved.set(true);
        this.successMessage = 'Perfil actualizado correctamente';
        setTimeout(() => {
          this.saved.set(false);
          this.successMessage = '';
        }, 3000);
      },
      error: (e) => {
        this.saving = false;
        this.error = 'Error al guardar los cambios';
      },
    });
  }

  onLogoUrlChange(): void {
    // Preview is handled by ngModel binding
  }

  onCoverUrlChange(): void {
    // Preview is handled by ngModel binding
  }
}