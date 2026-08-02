import { BusinessHour } from '../services/restaurant.service';

export interface OpenNowResult {
  isOpen: boolean | null;
  label: string;
  summary?: string;
}

const DAY_LABELS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

export function hasBusinessHours(hours: BusinessHour[] | undefined | null): hours is BusinessHour[] {
  return Array.isArray(hours) && hours.length > 0;
}

export function minutesOfDay(time: string): number {
  const [h = '0', m = '0', s = '0'] = time.split(':');
  return Number(h) * 60 + Number(m) + Number(s) / 60;
}

export function formatHour(time: string): string {
  const minutes = minutesOfDay(time);
  const h = Math.floor(minutes / 60) % 24;
  const m = Math.floor(minutes % 60);
  const suffix = h >= 12 ? 'PM' : 'AM';
  const h12 = h % 12 === 0 ? 12 : h % 12;
  return `${h12}:${m.toString().padStart(2, '0')} ${suffix}`;
}

/**
 * Replicates the backend BusinessHoursHelper (Restaurante.Domain/Helpers):
 * - No hours configured (empty/undefined) => null ("not applicable").
 * - Day with no configuration => null (schedule does not cover it).
 * - Day marked IsClosed => false.
 * - Overnight schedules (closeTime <= openTime) cross midnight: open when
 *   time >= openTime OR time < closeTime.
 * - Otherwise open when openTime <= time < closeTime.
 *
 * Midnight crossover is checked from BOTH sides: an overnight entry stored on
 * the previous day (e.g. Monday 22:00 - 02:00) still counts as open early the
 * next day (Tuesday 01:30).
 */
export function isOpenNow(hours: BusinessHour[] | undefined | null, now = new Date()): boolean | null {
  if (!hasBusinessHours(hours)) return null;

  const dayOfWeek = now.getDay(); // 0 = Sunday (matches backend dayOfWeek)
  const minutes = now.getHours() * 60 + now.getMinutes();

  return isOpenOn(hours, dayOfWeek, minutes);
}

export function isOpenOn(hours: BusinessHour[] | undefined | null, dayOfWeek: number, minutes: number): boolean | null {
  if (!hasBusinessHours(hours)) return null;

  const hour = hours.find(h => h.dayOfWeek === dayOfWeek);
  if (hour) {
    if (hour.isClosed) return false;

    const open = minutesOfDay(hour.openTime);
    const close = minutesOfDay(hour.closeTime);

    if (close <= open) return minutes >= open || minutes < close;
    return minutes >= open && minutes < close;
  }

  // The current day has no schedule entry: a restaurant that opens before
  // midnight may still be open right after midnight (overnight entry on the
  // previous day, e.g. Monday 22:00 - 02:00 covering Tuesday 01:30).
  const prevDay = (dayOfWeek + 6) % 7;
  const prev = hours.find(h => h.dayOfWeek === prevDay);
  if (prev && !prev.isClosed) {
    const open = minutesOfDay(prev.openTime);
    const close = minutesOfDay(prev.closeTime);
    if (close <= open && minutes < close) return true;
  }

  return null;
}

export function openNowSummary(hours: BusinessHour[] | undefined | null, now = new Date()): OpenNowResult | null {
  if (!hasBusinessHours(hours)) return null;
  const open = isOpenNow(hours, now);
  if (open === null) return null;
  return { isOpen: open, label: open ? 'Abierto ahora' : 'Cerrado' };
}

export function hoursLabel(hours: BusinessHour[] | undefined | null, now = new Date()): string {
  if (!hasBusinessHours(hours)) return 'Horario no publicado';
  const day = now.getDay();
  const hour = hours.find(h => h.dayOfWeek === day);
  if (!hour) return 'Sin horario hoy';
  if (hour.isClosed) return 'Cerrado hoy';
  const overnight = minutesOfDay(hour.closeTime) <= minutesOfDay(hour.openTime);
  return `${DAY_LABELS[hour.dayOfWeek]}: ${formatHour(hour.openTime)} - ${formatHour(hour.closeTime)}${overnight ? ' (al día siguiente)' : ''}`;
}

export function isOpenNowLabel(hours: BusinessHour[] | undefined | null, now = new Date()): string {
  const open = isOpenNow(hours, now);
  if (open === null) return 'Horario no publicado';
  return open ? 'Abierto ahora' : 'Cerrado';
}
