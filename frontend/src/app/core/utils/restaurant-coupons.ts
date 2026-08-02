import { z } from 'zod';
import { formatMoney } from './restaurant-dashboard';

const emptyToUndefined = (v: unknown) => (v === '' || v === null ? undefined : v);

/**
 * Coupon editor schema. Mirrors the backend validators:
 * - code required, max 50 chars, stored uppercase
 * - discountType is `Percentage` or `Fixed` (backend rejects "FixedAmount")
 * - discountValue > 0; validFrom/validUntil required, until after from
 * - maxUses/minOrderAmount optional
 */
export const couponFormSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, 'El código es obligatorio')
    .max(50, 'Máximo 50 caracteres')
    .transform(v => v.toUpperCase()),
  discountType: z.enum(['Percentage', 'Fixed'], { message: 'Seleccioná un tipo de descuento' }),
  discountValue: z.preprocess(
    (v) => Number(v),
    z.number({ message: 'Ingresá un valor válido' }).positive('Debe ser mayor a 0'),
  ),
  validFrom: z.string().min(1, 'La fecha de inicio es obligatoria'),
  validUntil: z.string().min(1, 'La fecha de fin es obligatoria'),
  maxUses: z.preprocess(
    emptyToUndefined,
    z.number({ message: 'Ingresá un número válido' }).int('Debe ser un número entero').positive('Debe ser mayor a 0').optional(),
  ),
  minOrderAmount: z.preprocess(
    emptyToUndefined,
    z.number({ message: 'Ingresá un número válido' }).min(0, 'No puede ser negativa').optional(),
  ),
  isActive: z.boolean().optional(),
});

/** Cross-field check mirroring the backend rule "ValidUntil must be after ValidFrom". */
export function validateCouponRange(from: string, until: string): string | null {
  if (!from || !until) return null;
  if (new Date(until).getTime() <= new Date(from).getTime()) {
    return 'La fecha de fin debe ser posterior al inicio';
  }
  return null;
}

export type CouponForm = z.infer<typeof couponFormSchema>;

/** `<input type="datetime-local">` value -> ISO 8601 UTC string (backend DateTime). */
export function toIsoLocal(value: string): string {
  if (!value) return '';
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? '' : d.toISOString();
}

/** ISO 8601 (from the API) -> local `datetime-local` input value. */
export function fromIsoToLocalInput(value: string): string {
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** Form values -> backend CreateCouponDto. */
export function toCreateCouponPayload(form: CouponForm): {
  code: string;
  discountType: 'Percentage' | 'Fixed';
  discountValue: number;
  validFrom: string;
  validUntil: string;
  maxUses: number | null;
  minOrderAmount: number | null;
} {
  return {
    code: form.code.toUpperCase(),
    discountType: form.discountType,
    discountValue: form.discountValue,
    validFrom: toIsoLocal(form.validFrom),
    validUntil: toIsoLocal(form.validUntil),
    maxUses: form.maxUses ?? null,
    minOrderAmount: form.minOrderAmount ?? null,
  };
}

/** Form values -> backend UpdateCouponDto (code/discountType are NOT updatable). */
export function toUpdateCouponPayload(form: CouponForm): {
  discountValue: number;
  validFrom: string;
  validUntil: string;
  maxUses: number | null;
  minOrderAmount: number | null;
  isActive: boolean;
} {
  return {
    discountValue: form.discountValue,
    validFrom: toIsoLocal(form.validFrom),
    validUntil: toIsoLocal(form.validUntil),
    maxUses: form.maxUses ?? null,
    minOrderAmount: form.minOrderAmount ?? null,
    isActive: form.isActive ?? true,
  };
}

export interface CouponLike {
  code: string;
  discountType: string;
  discountValue: number;
  validFrom: string;
  validUntil: string;
  maxUses?: number | null;
  minOrderAmount: number;
  isActive: boolean;
}

/** CouponDto -> editor form values. */
export function couponToForm(coupon: CouponLike): CouponForm {
  return {
    code: coupon.code,
    discountType: coupon.discountType === 'Percentage' ? 'Percentage' : 'Fixed',
    discountValue: coupon.discountValue,
    validFrom: fromIsoToLocalInput(coupon.validFrom),
    validUntil: fromIsoToLocalInput(coupon.validUntil),
    maxUses: coupon.maxUses ?? undefined,
    minOrderAmount: coupon.minOrderAmount > 0 ? coupon.minOrderAmount : undefined,
    isActive: coupon.isActive,
  };
}

/** "10%" for Percentage, "$5.00" for Fixed. */
export function discountLabel(coupon: { discountType: string; discountValue: number }): string {
  return coupon.discountType === 'Percentage' ? `${coupon.discountValue}%` : formatMoney(coupon.discountValue);
}

export interface CouponStatusMeta {
  label: string;
  badgeClass: string;
}

/** Activo / Expirado / Inactivo, with the matching semantic badge. */
export function couponStatusMeta(coupon: { isActive: boolean; validUntil: string }): CouponStatusMeta {
  if (!coupon.isActive) return { label: 'Inactivo', badgeClass: 'badge-cancelled' };
  const expiresAt = new Date(coupon.validUntil).getTime();
  if (Number.isNaN(expiresAt) || expiresAt < Date.now()) return { label: 'Expirado', badgeClass: 'badge-pending' };
  return { label: 'Activo', badgeClass: 'badge-delivered' };
}

/** "3/50" when maxUses set, "3" otherwise. */
export function usesLabel(coupon: { timesUsed: number; maxUses?: number | null }): string {
  return coupon.maxUses ? `${coupon.timesUsed}/${coupon.maxUses}` : `${coupon.timesUsed}`;
}

/** "1/1/2026 → 31/12/2026" (local dates). */
export function validRangeLabel(coupon: { validFrom: string; validUntil: string }): string {
  const from = new Date(coupon.validFrom);
  const until = new Date(coupon.validUntil);
  if (Number.isNaN(from.getTime()) || Number.isNaN(until.getTime())) return '—';
  return `${from.toLocaleDateString('es-AR')} → ${until.toLocaleDateString('es-AR')}`;
}

/** First readable issue of a zod parse failure (shared with onboarding). */
export function couponFormError(error: z.ZodError): string {
  return error.issues[0]?.message ?? 'Datos inválidos';
}
