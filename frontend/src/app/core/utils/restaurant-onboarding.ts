import { z } from 'zod';
import { BusinessHour } from '../services/restaurant.service';
import { isApiErrorEnvelope } from '../services/api-response';

/** Editor row for a single day; times are `HH:mm` (native `<input type="time">` value). */
export interface BusinessHoursRow {
  dayOfWeek: number; // 0 = Sunday (matches backend)
  isClosed: boolean;
  openTime: string; // "HH:mm"
  closeTime: string; // "HH:mm"
}

export const DAY_LABELS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

/** Backend validator: ^[a-z0-9-]+$ */
export function slugify(name: string): string {
  return name
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

export const slugSchema = z
  .string()
  .regex(/^[a-z0-9-]+$/, 'El slug solo puede contener minúsculas, números y guiones');

/** Backend validator: ^\+?[0-9\s\-()]{7,20}$ */
const phoneSchema = z
  .string()
  .regex(/^\+?[0-9\s\-()]{7,20}$/, 'Ingresá un teléfono válido');

const emptyToUndefined = (v: unknown) => (v === '' || v === null ? undefined : v);

export const basicInfoSchema = z.object({
  name: z.string().trim().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
  slug: slugSchema,
  description: z.string().max(500, 'Máximo 500 caracteres').optional(),
  phone: z.preprocess(emptyToUndefined, phoneSchema.optional()),
});

export const locationSchema = z.object({
  latitude: z.preprocess(
    (v) => (emptyToUndefined(v) === undefined ? undefined : Number(v)),
    z.number().min(-90, 'Latitud entre -90 y 90').max(90, 'Latitud entre -90 y 90').optional(),
  ),
  longitude: z.preprocess(
    (v) => (emptyToUndefined(v) === undefined ? undefined : Number(v)),
    z.number().min(-180, 'Longitud entre -180 y 180').max(180, 'Longitud entre -180 y 180').optional(),
  ),
});

const optionalPositive = (min: number, message: string) =>
  z.preprocess(
    (v) => (emptyToUndefined(v) === undefined ? undefined : Number(v)),
    z.number({ message }).min(min, message).optional(),
  );

export const deliverySchema = z.object({
  deliveryFee: z.preprocess((v) => Number(v), z.number().min(0, 'El costo de envío no puede ser negativo')),
  minOrderAmount: z.preprocess((v) => Number(v), z.number().min(0, 'El pedido mínimo no puede ser negativo')),
  radiusKm: optionalPositive(0.01, 'El radio debe ser mayor a 0'),
  estimatedPrepTimeMinutes: optionalPositive(1, 'El tiempo de preparación debe ser mayor a 0'),
});

/** "09:00" -> "09:00:00" (backend serializes TimeSpan as HH:mm:ss). */
export function toSecondsTime(time: string): string {
  return time.length === 5 ? `${time}:00` : time;
}

/** Seven rows, all open 09:00-22:00 (backend default when no hours configured). */
export function defaultBusinessHours(): BusinessHour[] {
  return Array.from({ length: 7 }, (_, dayOfWeek) => ({
    dayOfWeek,
    openTime: '09:00:00',
    closeTime: '22:00:00',
    isClosed: false,
  }));
}

/**
 * Map a restaurant's BusinessHour[] (from the API) onto 7 editor rows.
 * Days without a configured entry fall back to the default 09:00-22:00.
 */
export function hoursRowsFromBusinessHours(hours?: BusinessHour[] | null): BusinessHoursRow[] {
  return Array.from({ length: 7 }, (_, dayOfWeek) => {
    const existing = hours?.find(h => h.dayOfWeek === dayOfWeek);
    return {
      dayOfWeek,
      isClosed: existing ? existing.isClosed : false,
      openTime: existing ? existing.openTime.slice(0, 5) : '09:00',
      closeTime: existing ? existing.closeTime.slice(0, 5) : '22:00',
    };
  });
}

/** Editor rows -> backend payload ({ dayOfWeek, openTime "HH:mm:ss", closeTime, isClosed }), one entry per day. */
export function businessHoursFromRows(rows: BusinessHoursRow[]): BusinessHour[] {
  return rows.map(row => ({
    dayOfWeek: row.dayOfWeek,
    openTime: toSecondsTime(row.openTime),
    closeTime: toSecondsTime(row.closeTime),
    isClosed: row.isClosed,
  }));
}

/**
 * Backend rejects OpenTime == CloseTime on open days; validate before sending.
 * Returns a human-readable error or null when the editor is valid.
 */
export function validateHoursRows(rows: BusinessHoursRow[]): string | null {
  for (const row of rows) {
    if (!row.isClosed && row.openTime === row.closeTime) {
      return `${DAY_LABELS[row.dayOfWeek]}: la hora de apertura y cierre no pueden ser iguales`;
    }
  }
  return null;
}

/** First readable issue of a zod parse failure. */
export function firstIssue(error: z.ZodError): string {
  return error.issues[0]?.message ?? 'Datos inválidos';
}

/** Readable message from an API failure (envelope thrown by the service, or an HttpErrorResponse). */
export function readableApiError(err: unknown, fallback: string): string {
  if (isApiErrorEnvelope(err)) return err.message;
  const candidate = (err as any)?.error;
  if (isApiErrorEnvelope(candidate)) return candidate.message;
  const message = (err as any)?.error?.message;
  if (typeof message === 'string' && message) return message;
  return fallback;
}
