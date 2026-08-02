import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Restaurant, RestaurantService } from '../../../core/services/restaurant.service';
import { Coupon, CouponService } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  CouponForm,
  couponFormError,
  couponFormSchema,
  couponStatusMeta,
  couponToForm,
  discountLabel,
  toCreateCouponPayload,
  toUpdateCouponPayload,
  usesLabel,
  validRangeLabel,
  validateCouponRange,
} from '../../../core/utils/restaurant-coupons';
import { readableApiError } from '../../../core/utils/restaurant-onboarding';

@Component({
  selector: 'app-coupons',
  imports: [FormsModule, RouterLink],
  templateUrl: './coupons.html',
  styleUrl: './coupons.css',
})
export class Coupons implements OnInit {
  restaurants: Restaurant[] = [];
  restaurantId = '';
  coupons: Coupon[] = [];
  loading = true;
  saving = false;
  error = '';

  showForm = false;
  editing: Coupon | null = null;
  formError = '';
  form: CouponForm = this.emptyForm();

  constructor(
    private restaurantService: RestaurantService,
    private couponService: CouponService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        this.restaurants = restaurants;
        if (restaurants.length > 0) {
          this.restaurantId = restaurants[0].id;
          this.loadCoupons();
        }
        this.loading = false;
      },
      error: (e) => {
        this.error = readableApiError(e, 'No se pudieron cargar tus restaurantes');
        this.loading = false;
      },
    });
  }

  onBranchChange(id: string): void {
    this.restaurantId = id;
    this.coupons = [];
    this.loading = true;
    this.loadCoupons();
  }

  private loadCoupons(): void {
    if (!this.restaurantId) return;
    this.couponService.getRestaurantCoupons(this.restaurantId).subscribe({
      next: (data) => {
        this.coupons = data;
        this.loading = false;
      },
      error: (e) => {
        this.error = readableApiError(e, 'No se pudieron cargar los cupones');
        this.loading = false;
      },
    });
  }

  private emptyForm(): CouponForm {
    return {
      code: '',
      discountType: 'Percentage',
      discountValue: 0,
      validFrom: '',
      validUntil: '',
      maxUses: undefined,
      minOrderAmount: undefined,
      isActive: true,
    };
  }

  startCreate(): void {
    this.editing = null;
    this.form = this.emptyForm();
    this.formError = '';
    this.showForm = true;
  }

  startEdit(coupon: Coupon): void {
    this.editing = coupon;
    this.form = couponToForm(coupon);
    this.formError = '';
    this.showForm = true;
  }

  closeForm(): void {
    if (this.saving) return;
    this.showForm = false;
    this.editing = null;
    this.formError = '';
  }

  onTypeChange(type: 'Percentage' | 'Fixed'): void {
    this.form.discountType = type;
    this.form.discountValue = 0;
  }

  save(): void {
    const parsed = couponFormSchema.safeParse(this.form);
    if (!parsed.success) {
      this.formError = couponFormError(parsed.error);
      return;
    }
    const rangeError = validateCouponRange(parsed.data.validFrom, parsed.data.validUntil);
    if (rangeError) {
      this.formError = rangeError;
      return;
    }

    this.saving = true;
    this.formError = '';
    const submit = this.editing
      ? this.couponService.updateCoupon(this.restaurantId, this.editing.id, toUpdateCouponPayload(parsed.data))
      : this.couponService.createCoupon(this.restaurantId, toCreateCouponPayload(parsed.data));

    submit.subscribe({
      next: () => {
        this.toast.show(this.editing ? 'Cupón actualizado' : 'Cupón creado', 'success');
        this.saving = false;
        this.showForm = false;
        this.editing = null;
        this.loadCoupons();
      },
      error: (e) => {
        this.saving = false;
        this.formError = readableApiError(e, 'No se pudo guardar el cupón');
      },
    });
  }

  deleteCoupon(coupon: Coupon): void {
    if (!confirm(`¿Eliminar el cupón ${coupon.code}?`)) return;
    this.couponService.deleteCoupon(this.restaurantId, coupon.id).subscribe({
      next: () => {
        this.toast.show(`Cupón ${coupon.code} eliminado`, 'success');
        this.loadCoupons();
      },
      error: (e) => {
        this.toast.show(readableApiError(e, 'No se pudo eliminar el cupón'), 'error');
      },
    });
  }

  protected readonly discountLabel = discountLabel;
  protected readonly couponStatusMeta = couponStatusMeta;
  protected readonly usesLabel = usesLabel;
  protected readonly validRangeLabel = validRangeLabel;
}
