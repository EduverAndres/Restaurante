import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';
import {
  slugify,
  basicInfoSchema,
  locationSchema,
  deliverySchema,
  hoursRowsFromBusinessHours,
  businessHoursFromRows,
  validateHoursRows,
  firstIssue,
  readableApiError,
  BusinessHoursRow,
  DAY_LABELS,
} from '../../../core/utils/restaurant-onboarding';

const parseOrNull = (value: string): number | null => (value === '' ? null : Number(value));

@Component({
  selector: 'app-restaurant-settings',
  imports: [FormsModule, RouterLink],
  templateUrl: './settings.html',
  styleUrl: './settings.css',
})
export class Settings implements OnInit {
  restaurant: Restaurant | null = null;
  loading = true;
  locating = signal(false);

  savingBasic = signal(false);
  savingHours = signal(false);
  savingDelivery = signal(false);
  uploading = signal<'logo' | 'cover' | null>(null);

  errorBasic = signal('');
  errorHours = signal('');
  errorDelivery = signal('');
  errorImages = signal('');

  // Basic info + location
  name = '';
  slug = '';
  slugTouched = false;
  description = '';
  phone = '';
  latitude = '';
  longitude = '';

  // Delivery
  deliveryFee = '0';
  minOrderAmount = '0';
  radiusKm = '';
  estimatedPrepTimeMinutes = '';

  // Business hours
  hoursRows: BusinessHoursRow[] = hoursRowsFromBusinessHours(null);

  // Images
  logoPreview = '';
  coverPreview = '';
  logoFile: File | null = null;
  coverFile: File | null = null;

  readonly dayLabels = DAY_LABELS;

  constructor(
    private restaurantService: RestaurantService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (list) => {
        if (list.length === 0) {
          this.loading = false;
          return;
        }
        // The owner list is lightweight (no hours/delivery); fetch the full detail.
        this.restaurantService.getById(list[0].id).subscribe({
          next: (restaurant) => {
            this.restaurant = restaurant;
            this.populate();
            this.loading = false;
          },
          error: () => (this.loading = false),
        });
      },
      error: () => (this.loading = false),
    });
  }

  private populate(): void {
    const r = this.restaurant;
    if (!r) return;
    this.name = r.name;
    this.slug = r.slug;
    this.description = r.description || '';
    this.phone = r.phone || '';
    this.latitude = r.latitude !== undefined && r.latitude !== null ? String(r.latitude) : '';
    this.longitude = r.longitude !== undefined && r.longitude !== null ? String(r.longitude) : '';
    this.deliveryFee = r.deliveryFee !== undefined ? String(r.deliveryFee) : '0';
    this.minOrderAmount = r.minOrderAmount !== undefined ? String(r.minOrderAmount) : '0';
    this.radiusKm = r.radiusKm !== undefined && r.radiusKm !== null ? String(r.radiusKm) : '';
    this.estimatedPrepTimeMinutes =
      r.estimatedPrepTimeMinutes !== undefined && r.estimatedPrepTimeMinutes !== null ? String(r.estimatedPrepTimeMinutes) : '';
    this.hoursRows = hoursRowsFromBusinessHours(r.businessHours);
    this.logoPreview = r.logo || '';
    this.coverPreview = r.coverImage || '';
  }

  onNameChange(): void {
    if (!this.slugTouched) {
      this.slug = slugify(this.name);
    }
  }

  onSlugInput(): void {
    this.slugTouched = true;
  }

  regenerateSlug(): void {
    this.slug = slugify(this.name);
    this.slugTouched = false;
  }

  useMyLocation(): void {
    if (!navigator.geolocation) {
      this.errorBasic.set('Tu navegador no soporta geolocalización.');
      return;
    }
    this.locating.set(true);
    this.errorBasic.set('');
    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.latitude = position.coords.latitude.toFixed(6);
        this.longitude = position.coords.longitude.toFixed(6);
        this.locating.set(false);
      },
      () => {
        this.locating.set(false);
        this.errorBasic.set('No se pudo obtener tu ubicación.');
      },
      { enableHighAccuracy: true, timeout: 10000 },
    );
  }

  saveBasic(): void {
    const info = basicInfoSchema.safeParse({ name: this.name, slug: this.slug, description: this.description, phone: this.phone });
    if (!info.success) {
      this.errorBasic.set(firstIssue(info.error));
      return;
    }
    const location = locationSchema.safeParse({ latitude: this.latitude, longitude: this.longitude });
    if (!location.success) {
      this.errorBasic.set(firstIssue(location.error));
      return;
    }

    const r = this.restaurant;
    if (!r) return;
    this.savingBasic.set(true);
    this.errorBasic.set('');

    // PUT replaces the whole record: echo back fields this section does not edit
    // (logo/coverImage/themeConfig/isActive) so they are not wiped out.
    const themeConfig = typeof (r as any).themeConfig === 'string' ? (r as any).themeConfig : null;
    const payload: any = {
      name: this.name.trim(),
      slug: this.slug,
      description: this.description.trim() || null,
      phone: this.phone.trim() || null,
      latitude: parseOrNull(this.latitude),
      longitude: parseOrNull(this.longitude),
      logo: r.logo ?? null,
      coverImage: r.coverImage ?? null,
      themeConfig,
      isActive: r.isActive ?? true,
    };

    this.restaurantService.update(r.id, payload).subscribe({
      next: (updated) => {
        this.restaurant = updated;
        this.savingBasic.set(false);
        this.toast.show('Información guardada', 'success');
      },
      error: (err) => {
        this.savingBasic.set(false);
        this.errorBasic.set(readableApiError(err, 'No se pudieron guardar los cambios.'));
      },
    });
  }

  saveHours(): void {
    const issue = validateHoursRows(this.hoursRows);
    if (issue) {
      this.errorHours.set(issue);
      return;
    }
    const r = this.restaurant;
    if (!r) return;
    this.savingHours.set(true);
    this.errorHours.set('');

    this.restaurantService.updateBusinessHours(r.id, businessHoursFromRows(this.hoursRows)).subscribe({
      next: (updated) => {
        this.restaurant = updated;
        this.savingHours.set(false);
        this.toast.show('Horarios guardados', 'success');
      },
      error: (err) => {
        this.savingHours.set(false);
        this.errorHours.set(readableApiError(err, 'No se pudieron guardar los horarios.'));
      },
    });
  }

  saveDelivery(): void {
    const result = deliverySchema.safeParse({
      deliveryFee: this.deliveryFee,
      minOrderAmount: this.minOrderAmount,
      radiusKm: this.radiusKm,
      estimatedPrepTimeMinutes: this.estimatedPrepTimeMinutes,
    });
    if (!result.success) {
      this.errorDelivery.set(firstIssue(result.error));
      return;
    }
    const r = this.restaurant;
    if (!r) return;
    this.savingDelivery.set(true);
    this.errorDelivery.set('');

    this.restaurantService.updateDeliverySettings(r.id, {
      deliveryFee: Number(this.deliveryFee) || 0,
      minOrderAmount: Number(this.minOrderAmount) || 0,
      radiusKm: parseOrNull(this.radiusKm),
      estimatedPrepTimeMinutes: parseOrNull(this.estimatedPrepTimeMinutes),
    }).subscribe({
      next: (updated) => {
        this.restaurant = updated;
        this.savingDelivery.set(false);
        this.toast.show('Configuración de envío guardada', 'success');
      },
      error: (err) => {
        this.savingDelivery.set(false);
        this.errorDelivery.set(readableApiError(err, 'No se pudo guardar la configuración de envío.'));
      },
    });
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.logoFile = file;
    const reader = new FileReader();
    reader.onload = () => (this.logoPreview = reader.result as string);
    reader.readAsDataURL(file);
  }

  onCoverSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.coverFile = file;
    const reader = new FileReader();
    reader.onload = () => (this.coverPreview = reader.result as string);
    reader.readAsDataURL(file);
  }

  uploadLogo(): void {
    if (!this.logoFile || !this.restaurant) return;
    this.upload('logo', this.logoFile);
  }

  uploadCover(): void {
    if (!this.coverFile || !this.restaurant) return;
    this.upload('cover', this.coverFile);
  }

  private upload(type: 'logo' | 'cover', file: File): void {
    const r = this.restaurant;
    if (!r) return;
    this.uploading.set(type);
    this.errorImages.set('');

    this.restaurantService.uploadImage(r.id, type, file).subscribe({
      next: (updated) => {
        this.restaurant = updated;
        this.logoFile = null;
        this.coverFile = null;
        this.logoPreview = updated.logo || this.logoPreview;
        this.coverPreview = updated.coverImage || this.coverPreview;
        this.uploading.set(null);
        this.toast.show(type === 'logo' ? 'Logo actualizado' : 'Portada actualizada', 'success');
      },
      error: (err) => {
        this.uploading.set(null);
        this.errorImages.set(readableApiError(err, 'No se pudo subir la imagen.'));
      },
    });
  }
}
