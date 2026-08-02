import { describe, it, expect } from 'vitest';
import {
  hasBusinessHours,
  minutesOfDay,
  formatHour,
  isOpenNow,
  isOpenOn,
  openNowSummary,
  hoursLabel,
} from './business-hours';
import { BusinessHour } from '../services/restaurant.service';

function day(dayOfWeek: number, open: string, close: string, isClosed = false): BusinessHour {
  return { dayOfWeek, openTime: open, closeTime: close, isClosed };
}

const FULL_WEEK: BusinessHour[] = [
  day(0, '09:00:00', '23:00:00'),
  day(1, '09:00:00', '23:00:00'),
  day(2, '09:00:00', '23:00:00'),
  day(3, '09:00:00', '23:00:00'),
  day(4, '09:00:00', '23:00:00'),
  day(5, '09:00:00', '23:00:00'),
  day(6, '09:00:00', '23:00:00'),
];

// 2026-08-02 is a Sunday (getDay() === 0), 2026-08-03 is Monday.
const sundayNoon = new Date(2026, 7, 2, 12, 0, 0);
const mondayNoon = new Date(2026, 7, 3, 12, 0, 0);
const mondayNight = new Date(2026, 7, 3, 23, 30, 0);

describe('minutesOfDay', () => {
  it('parses HH:mm:ss into minutes', () => {
    expect(minutesOfDay('09:30:00')).toBe(570);
  });

  it('handles missing segments', () => {
    expect(minutesOfDay('09')).toBe(540);
    expect(minutesOfDay('09:30')).toBe(570);
  });
});

describe('formatHour', () => {
  it('formats 24h times with AM/PM', () => {
    expect(formatHour('09:00:00')).toBe('9:00 AM');
    expect(formatHour('23:00:00')).toBe('11:00 PM');
    expect(formatHour('00:30:00')).toBe('12:30 AM');
  });
});

describe('hasBusinessHours', () => {
  it('rejects undefined, null and empty arrays', () => {
    expect(hasBusinessHours(undefined)).toBe(false);
    expect(hasBusinessHours(null)).toBe(false);
    expect(hasBusinessHours([])).toBe(false);
  });

  it('accepts a non-empty array', () => {
    expect(hasBusinessHours(FULL_WEEK)).toBe(true);
  });
});

describe('isOpenNow', () => {
  it('returns null when no hours are configured', () => {
    expect(isOpenNow(undefined, mondayNoon)).toBeNull();
    expect(isOpenNow([], mondayNoon)).toBeNull();
  });

  it('is open within business hours', () => {
    expect(isOpenNow(FULL_WEEK, mondayNoon)).toBe(true);
  });

  it('is closed before open or after close', () => {
    expect(isOpenNow(FULL_WEEK, new Date(2026, 7, 3, 8, 0, 0))).toBe(false);
    expect(isOpenNow(FULL_WEEK, mondayNight)).toBe(false);
  });

  it('returns null when the day has no schedule entry', () => {
    const onlySunday = [day(0, '09:00:00', '23:00:00')];
    expect(isOpenNow(onlySunday, mondayNoon)).toBeNull();
  });

  it('returns false on a day marked closed', () => {
    const closedSunday = FULL_WEEK.map(h => (h.dayOfWeek === 0 ? { ...h, isClosed: true } : h));
    expect(isOpenNow(closedSunday, sundayNoon)).toBe(false);
  });

  it('treats dayOfWeek 0 as Sunday', () => {
    expect(isOpenNow(FULL_WEEK, sundayNoon)).toBe(true);
  });

  describe('overnight schedules (crossing midnight)', () => {
    const overnight = [day(1, '22:00:00', '02:00:00')];

    it('is open after opening time the same night', () => {
      expect(isOpenNow(overnight, new Date(2026, 7, 3, 23, 0, 0))).toBe(true);
    });

    it('is open after midnight before close', () => {
      expect(isOpenNow(overnight, new Date(2026, 7, 4, 1, 30, 0))).toBe(true);
    });

    it('is closed in the middle of the day', () => {
      expect(isOpenNow(overnight, mondayNoon)).toBe(false);
    });
  });
});

describe('isOpenOn', () => {
  it('is open with explicit day and minutes', () => {
    expect(isOpenOn(FULL_WEEK, 1, 720)).toBe(true);
    expect(isOpenOn(FULL_WEEK, 1, 1380)).toBe(false);
  });

  it('handles overnight ranges explicitly', () => {
    const overnight = [day(2, '22:00:00', '02:00:00')];
    expect(isOpenOn(overnight, 2, 1380)).toBe(true);
    expect(isOpenOn(overnight, 2, 60)).toBe(true);
    expect(isOpenOn(overnight, 2, 720)).toBe(false);
  });
});

describe('openNowSummary', () => {
  it('labels open and closed states', () => {
    expect(openNowSummary(FULL_WEEK, mondayNoon)?.isOpen).toBe(true);
    expect(openNowSummary(FULL_WEEK, mondayNoon)?.label).toBe('Abierto ahora');
    expect(openNowSummary(FULL_WEEK, mondayNight)?.label).toBe('Cerrado');
    expect(openNowSummary(undefined, mondayNoon)).toBeNull();
  });
});

describe('hoursLabel', () => {
  it('shows the schedule for today', () => {
    expect(hoursLabel(FULL_WEEK, mondayNoon)).toBe('Lunes: 9:00 AM - 11:00 PM');
  });

  it('marks overnight schedules', () => {
    const overnight = [day(1, '22:00:00', '02:00:00')];
    expect(hoursLabel(overnight, mondayNoon)).toContain('(al día siguiente)');
  });

  it('handles missing and closed days', () => {
    expect(hoursLabel(undefined, mondayNoon)).toBe('Horario no publicado');
    const closed = [day(1, '09:00:00', '23:00:00', true)];
    expect(hoursLabel(closed, mondayNoon)).toBe('Cerrado hoy');
  });
});
