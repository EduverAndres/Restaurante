import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { concatMap, of } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
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

const STEP_LABELS = ['Información básica', 'Ubicación', 'Envío', 'Horarios', 'Imágenes'];

const parseOrNull = (value: string): number | null => (value === '' ? null : Number(value));

@Component({
  selector: 'app-restaurant-onboarding',
  imports: [FormsModule],
  templateUrl: './onboarding.html',
  styleUrl: './onboarding.css',
})
export class Onboarding implements OnInit {
  step = signal(1);
  submitting = signal(false);
  locating = signal(false);
  error = signal('');

  // Step 1 — basic info
  name = '';
  slug = '';
  slugTouched = false;
  description = '';
  phone = '';

  // Step 2 — location
  latitude = '';
  longitude = '';

  // Step 3 — delivery
  deliveryFee = '3.50';
  minOrderAmount = '10.00';
  radiusKm = '';
  estimatedPrepTimeMinutes = '';

  // Step 4 — business hours (7 rows, 0 = Sunday)
  hoursRows: BusinessHoursRow[] = hoursRowsFromBusinessHours(null);

  // Step 5 — images
  logoFile: File | null = null;
  coverFile: File | null = null;
  logoPreview = '';
  coverPreview = '';

  readonly steps = STEP_LABELS;
  readonly dayLabels = DAY_LABELS;

  constructor(
    private router: Router,
    private restaurantService: RestaurantService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    // Owner that already has a store does not go through the wizard again.
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.toast.show('Ya tenés una tienda configurada. Podés editarla desde Configuración.', 'info');
          this.router.navigate(['/restaurant/dashboard']);
        }
      },
    });
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
      this.error.set('Tu navegador no soporta geolocalización. Ingresá las coordenadas manualmente.');
      return;
    }
    this.locating.set(true);
    this.error.set('');
    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.latitude = position.coords.latitude.toFixed(6);
        this.longitude = position.coords.longitude.toFixed(6);
        this.locating.set(false);
      },
      () => {
        this.locating.set(false);
        this.error.set('No se pudo obtener tu ubicación. Ingresala manualmente.');
      },
      { enableHighAccuracy: true, timeout: 10000 },
    );
  }

  goToStep(target: number): void {
    this.error.set('');
    this.step.set(Math.min(Math.max(target, 1), STEP_LABELS.length));
  }

  next(): void {
    const issue = this.validateStep(this.step());
    if (issue) {
      this.error.set(issue);
      return;
    }
    this.error.set('');
    this.step.update(s => Math.min(s + 1, STEP_LABELS.length));
  }

  prev(): void {
    this.error.set('');
    this.step.update(s => Math.max(s - 1, 1));
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.logoFile = file;
    this.logoPreview = '';
    const reader = new FileReader();
    reader.onload = () => (this.logoPreview = reader.result as string);
    reader.readAsDataURL(file);
  }

  onCoverSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.coverFile = file;
    this.coverPreview = '';
    const reader = new FileReader();
    reader.onload = () => (this.coverPreview = reader.result as string);
    reader.readAsDataURL(file);
  }

  private validateStep(step: number): string | null {
    switch (step) {
      case 1: {
        const result = basicInfoSchema.safeParse({
          name: this.name,
          slug: this.slug,
          description: this.description,
          phone: this.phone,
        });
        return result.success ? null : firstIssue(result.error);
      }
      case 2: {
        const result = locationSchema.safeParse({ latitude: this.latitude, longitude: this.longitude });
        return result.success ? null : firstIssue(result.error);
      }
      case 3: {
        const result = deliverySchema.safeParse({
          deliveryFee: this.deliveryFee,
          minOrderAmount: this.minOrderAmount,
          radiusKm: this.radiusKm,
          estimatedPrepTimeMinutes: this.estimatedPrepTimeMinutes,
        });
        return result.success ? null : firstIssue(result.error);
      }
      case 4:
        return validateHoursRows(this.hoursRows);
      default:
        return null;
    }
  }

  submit(): void {
    // Re-validate every step before hitting the API.
    for (let s = 1; s <= STEP_LABELS.length; s++) {
      const issue = this.validateStep(s);
      if (issue) {
        this.error.set(issue);
        this.step.set(s);
        return;
      }
    }

    this.error.set('');
    this.submitting.set(true);

    const payload = {
      name: this.name.trim(),
      slug: this.slug,
      description: this.description.trim() || undefined,
      phone: this.phone.trim() || undefined,
      latitude: parseOrNull(this.latitude) ?? undefined,
      longitude: parseOrNull(this.longitude) ?? undefined,
      deliveryFee: Number(this.deliveryFee) || 0,
      minOrderAmount: Number(this.minOrderAmount) || 0,
      radiusKm: parseOrNull(this.radiusKm) ?? undefined,
      estimatedPrepTimeMinutes: parseOrNull(this.estimatedPrepTimeMinutes) ?? undefined,
    };

    this.restaurantService.create(payload).subscribe({
      next: (created) => this.finishSetup(created.id),
      error: (err) => {
        this.submitting.set(false);
        this.error.set(readableApiError(err, 'No se pudo crear la tienda. Intentá de nuevo.'));
      },
    });
  }

  /** Order matters: the restaurant must exist first, then hours, delivery and images. */
  private finishSetup(restaurantId: string): void {
    this.restaurantService.updateBusinessHours(restaurantId, businessHoursFromRows(this.hoursRows)).pipe(
      concatMap(() =>
        this.restaurantService.updateDeliverySettings(restaurantId, {
          deliveryFee: Number(this.deliveryFee) || 0,
          minOrderAmount: Number(this.minOrderAmount) || 0,
          radiusKm: parseOrNull(this.radiusKm),
          estimatedPrepTimeMinutes: parseOrNull(this.estimatedPrepTimeMinutes),
        }),
      ),
      concatMap(() => (this.logoFile ? this.restaurantService.uploadImage(restaurantId, 'logo', this.logoFile) : of(null))),
      concatMap(() => (this.coverFile ? this.restaurantService.uploadImage(restaurantId, 'cover', this.coverFile) : of(null))),
    ).subscribe({
      next: () => {
        this.submitting.set(false);
        this.toast.show('¡Tu tienda está lista!', 'success');
        this.router.navigate(['/restaurant/dashboard']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(readableApiError(err, 'La tienda se creó, pero hubo un error al configurarla.'));
      },
    });
  }
}
