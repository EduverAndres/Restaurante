import { describe, it, expect } from 'vitest';
import {
  slugify,
  slugSchema,
  basicInfoSchema,
  locationSchema,
  deliverySchema,
  toSecondsTime,
  defaultBusinessHours,
  hoursRowsFromBusinessHours,
  businessHoursFromRows,
  validateHoursRows,
  firstIssue,
  readableApiError,
  BusinessHoursRow,
} from './restaurant-onboarding';
import { BusinessHour } from '../services/restaurant.service';

describe('slugify', () => {
  it('lowercases and replaces spaces with dashes', () => {
    expect(slugify('Mi Restaurante')).toBe('mi-restaurante');
  });

  it('strips accents', () => {
    expect(slugify('Café Martínez')).toBe('cafe-martinez');
  });

  it('drops invalid characters and trims dashes', () => {
    expect(slugify('  Pizzas #1!!! ')).toBe('pizzas-1');
  });

  it('returns empty for empty input', () => {
    expect(slugify('')).toBe('');
  });
});

describe('slugSchema', () => {
  it('accepts lowercase letters, numbers and dashes', () => {
    expect(slugSchema.safeParse('el-rincon-2').success).toBe(true);
  });

  it('rejects uppercase, accents and spaces', () => {
    expect(slugSchema.safeParse('El Rincón').success).toBe(false);
    expect(slugSchema.safeParse('mi-tienda!').success).toBe(false);
  });
});

describe('basicInfoSchema', () => {
  it('accepts a valid payload', () => {
    const result = basicInfoSchema.safeParse({
      name: 'Mi Restaurante',
      slug: 'mi-restaurante',
      description: 'Comida casera',
      phone: '+54 11 5555 1234',
    });
    expect(result.success).toBe(true);
  });

  it('rejects a missing name', () => {
    const result = basicInfoSchema.safeParse({ name: '', slug: 'x' });
    expect(result.success).toBe(false);
    expect(firstIssue((result as any).error)).toContain('nombre');
  });

  it('rejects an invalid phone but allows empty', () => {
    expect(basicInfoSchema.safeParse({ name: 'A', slug: 'a', phone: 'abc' }).success).toBe(false);
    expect(basicInfoSchema.safeParse({ name: 'A', slug: 'a', phone: '' }).success).toBe(true);
    expect(basicInfoSchema.safeParse({ name: 'A', slug: 'a' }).success).toBe(true);
  });
});

describe('locationSchema', () => {
  it('accepts empty or valid coordinates', () => {
    expect(locationSchema.safeParse({ latitude: '', longitude: '' }).success).toBe(true);
    expect(locationSchema.safeParse({ latitude: -34.6, longitude: -58.4 }).success).toBe(true);
  });

  it('rejects out-of-range coordinates', () => {
    expect(locationSchema.safeParse({ latitude: 91, longitude: 0 }).success).toBe(false);
    expect(locationSchema.safeParse({ latitude: 0, longitude: 181 }).success).toBe(false);
  });
});

describe('deliverySchema', () => {
  it('accepts required fees and optional positive fields', () => {
    const result = deliverySchema.safeParse({ deliveryFee: 3.5, minOrderAmount: 10, radiusKm: '', estimatedPrepTimeMinutes: '' });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.radiusKm).toBeUndefined();
    }
  });

  it('rejects negative fees', () => {
    expect(deliverySchema.safeParse({ deliveryFee: -1, minOrderAmount: 10 }).success).toBe(false);
  });
});

describe('business hours helpers', () => {
  it('defaults to 7 days open 09:00-22:00 with HH:mm:ss', () => {
    const hours = defaultBusinessHours();
    expect(hours).toHaveLength(7);
    expect(hours[0]).toEqual({ dayOfWeek: 0, openTime: '09:00:00', closeTime: '22:00:00', isClosed: false });
  });

  it('maps API hours onto editor rows, falling back to defaults', () => {
    const api: BusinessHour[] = [
      { dayOfWeek: 1, openTime: '08:30:00', closeTime: '23:00:00', isClosed: false },
      { dayOfWeek: 2, openTime: '09:00:00', closeTime: '22:00:00', isClosed: true },
    ];
    const rows = hoursRowsFromBusinessHours(api);
    expect(rows).toHaveLength(7);
    expect(rows[1]).toEqual({ dayOfWeek: 1, isClosed: false, openTime: '08:30', closeTime: '23:00' });
    expect(rows[2].isClosed).toBe(true);
    expect(rows[0]).toEqual({ dayOfWeek: 0, isClosed: false, openTime: '09:00', closeTime: '22:00' });
  });

  it('produces an empty editor when hours are missing', () => {
    const rows = hoursRowsFromBusinessHours(null);
    expect(rows).toHaveLength(7);
    expect(rows.every(r => r.openTime === '09:00' && r.closeTime === '22:00' && !r.isClosed)).toBe(true);
  });

  it('converts editor rows to the backend payload with HH:mm:ss', () => {
    const rows: BusinessHoursRow[] = [
      { dayOfWeek: 0, isClosed: false, openTime: '09:00', closeTime: '22:00' },
      { dayOfWeek: 6, isClosed: true, openTime: '09:00', closeTime: '22:00' },
    ];
    const payload = businessHoursFromRows(rows);
    expect(payload).toEqual([
      { dayOfWeek: 0, openTime: '09:00:00', closeTime: '22:00:00', isClosed: false },
      { dayOfWeek: 6, openTime: '09:00:00', closeTime: '22:00:00', isClosed: true },
    ]);
  });

  it('toSecondsTime pads HH:mm and leaves HH:mm:ss untouched', () => {
    expect(toSecondsTime('09:00')).toBe('09:00:00');
    expect(toSecondsTime('09:00:00')).toBe('09:00:00');
  });

  it('validateHoursRows flags equal open/close times on open days', () => {
    const rows: BusinessHoursRow[] = hoursRowsFromBusinessHours(null);
    rows[3].openTime = '10:00';
    rows[3].closeTime = '10:00';
    expect(validateHoursRows(rows)).toContain('Miércoles');
    rows[3].isClosed = true;
    expect(validateHoursRows(rows)).toBeNull();
  });
});

describe('readableApiError', () => {
  it('reads the message from a thrown envelope', () => {
    expect(readableApiError({ success: false, message: 'Slug is already in use', data: null }, 'fallback')).toBe('Slug is already in use');
  });

  it('reads the message from an HttpErrorResponse shape', () => {
    const httpErr = { error: { success: false, message: 'Validation failed', data: null } };
    expect(readableApiError(httpErr, 'fallback')).toBe('Validation failed');
    expect(readableApiError({}, 'fallback')).toBe('fallback');
  });
});
