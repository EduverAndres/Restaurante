import { describe, expect, it } from 'vitest';
import {
  couponFormSchema,
  couponStatusMeta,
  couponToForm,
  discountLabel,
  fromIsoToLocalInput,
  toCreateCouponPayload,
  toIsoLocal,
  toUpdateCouponPayload,
  usesLabel,
  validateCouponRange,
  validRangeLabel,
} from './restaurant-coupons';

const validForm = {
  code: 'BIENVENIDO',
  discountType: 'Percentage',
  discountValue: 10,
  validFrom: '2026-08-01T10:00',
  validUntil: '2026-12-31T23:59',
  maxUses: '',
  minOrderAmount: '',
  isActive: true,
};

describe('couponFormSchema', () => {
  it('accepts a valid payload and uppercases the code', () => {
    const result = couponFormSchema.safeParse(validForm);
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.code).toBe('BIENVENIDO');
  });

  it('uppercases lowercase codes', () => {
    const result = couponFormSchema.safeParse({ ...validForm, code: 'hola10' });
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.code).toBe('HOLA10');
  });

  it('rejects missing code', () => {
    const result = couponFormSchema.safeParse({ ...validForm, code: '' });
    expect(result.success).toBe(false);
  });

  it('rejects invalid discount types (backend only allows Percentage|Fixed)', () => {
    const result = couponFormSchema.safeParse({ ...validForm, discountType: 'FixedAmount' });
    expect(result.success).toBe(false);
  });

  it('rejects non-positive discount values', () => {
    expect(couponFormSchema.safeParse({ ...validForm, discountValue: 0 }).success).toBe(false);
    expect(couponFormSchema.safeParse({ ...validForm, discountValue: -5 }).success).toBe(false);
  });

  it('rejects missing dates', () => {
    expect(couponFormSchema.safeParse({ ...validForm, validFrom: '' }).success).toBe(false);
    expect(couponFormSchema.safeParse({ ...validForm, validUntil: '' }).success).toBe(false);
  });

  it('rejects invalid optional numbers', () => {
    expect(couponFormSchema.safeParse({ ...validForm, maxUses: 0 }).success).toBe(false);
    expect(couponFormSchema.safeParse({ ...validForm, maxUses: 1.5 }).success).toBe(false);
    expect(couponFormSchema.safeParse({ ...validForm, minOrderAmount: -1 }).success).toBe(false);
  });

  it('accepts empty optional numbers', () => {
    expect(couponFormSchema.safeParse(validForm).success).toBe(true);
  });
});

describe('validateCouponRange', () => {
  it('rejects until <= from', () => {
    expect(validateCouponRange('2026-08-01T10:00', '2026-08-01T10:00')).not.toBeNull();
    expect(validateCouponRange('2026-08-01T10:00', '2026-07-01T10:00')).not.toBeNull();
  });

  it('accepts until > from', () => {
    expect(validateCouponRange('2026-08-01T10:00', '2026-08-02T10:00')).toBeNull();
  });
});

describe('payload mapping', () => {
  it('builds the backend CreateCouponDto with ISO dates', () => {
    const payload = toCreateCouponPayload(couponFormSchema.parse(validForm));
    expect(payload.code).toBe('BIENVENIDO');
    expect(payload.discountType).toBe('Percentage');
    expect(payload.discountValue).toBe(10);
    expect(payload.validFrom).toBe(new Date('2026-08-01T10:00').toISOString());
    expect(payload.maxUses).toBeNull();
    expect(payload.minOrderAmount).toBeNull();
  });

  it('builds the backend UpdateCouponDto (no code/discountType)', () => {
    const payload = toUpdateCouponPayload(couponFormSchema.parse(validForm));
    expect(payload).toEqual({
      discountValue: 10,
      validFrom: expect.any(String),
      validUntil: expect.any(String),
      maxUses: null,
      minOrderAmount: null,
      isActive: true,
    });
    expect('code' in payload).toBe(false);
  });

  it('round-trips a coupon DTO into form values', () => {
    const form = couponToForm({
      id: 'x',
      code: 'HOLA10',
      discountType: 'Percentage',
      discountValue: 10,
      validFrom: '2026-08-01T13:00:00Z',
      validUntil: '2026-12-31T23:59:00Z',
      maxUses: 50,
      minOrderAmount: 0,
      isActive: true,
    });
    expect(form.code).toBe('HOLA10');
    expect(form.discountType).toBe('Percentage');
    expect(form.maxUses).toBe(50);
    expect(form.minOrderAmount).toBeUndefined();
  });
});

describe('date input helpers', () => {
  it('converts local input to ISO and back', () => {
    const iso = toIsoLocal('2026-08-01T10:30');
    expect(iso).toBe(new Date('2026-08-01T10:30').toISOString());
    const local = fromIsoToLocalInput(iso);
    expect(local).toBe('2026-08-01T10:30');
  });

  it('handles invalid input', () => {
    expect(toIsoLocal('')).toBe('');
    expect(fromIsoToLocalInput('garbage')).toBe('');
  });
});

describe('display helpers', () => {
  it('formats the discount label per type', () => {
    expect(discountLabel({ discountType: 'Percentage', discountValue: 10 })).toBe('10%');
    expect(discountLabel({ discountType: 'Fixed', discountValue: 5 })).toBe('$5.00');
  });

  it('computes coupon status', () => {
    const active = couponStatusMeta({ isActive: true, validUntil: '2999-01-01T00:00:00Z' });
    expect(active.label).toBe('Activo');
    expect(active.badgeClass).toBe('badge-delivered');

    const inactive = couponStatusMeta({ isActive: false, validUntil: '2999-01-01T00:00:00Z' });
    expect(inactive.label).toBe('Inactivo');

    const expired = couponStatusMeta({ isActive: true, validUntil: '2020-01-01T00:00:00Z' });
    expect(expired.label).toBe('Expirado');
  });

  it('formats uses', () => {
    expect(usesLabel({ timesUsed: 3, maxUses: 50 })).toBe('3/50');
    expect(usesLabel({ timesUsed: 3, maxUses: null })).toBe('3');
  });

  it('formats the validity range', () => {
    const label = validRangeLabel({ validFrom: '2026-08-01T13:00:00Z', validUntil: '2026-12-31T23:00:00Z' });
    expect(label).toContain('→');
    expect(label).toContain('2026');
  });
});
