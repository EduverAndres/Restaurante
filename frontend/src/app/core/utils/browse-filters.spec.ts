import { describe, it, expect } from 'vitest';
import { Restaurant, BusinessHour } from '../services/restaurant.service';
import { applyBrowseFilters, hasSortData, hasOpenNowData, SortKey } from './browse-filters';

function hour(dayOfWeek: number, open: string, close: string): BusinessHour {
  return { dayOfWeek, openTime: open, closeTime: close, isClosed: false };
}

function makeRestaurant(partial: Partial<Restaurant> & { id: string; name: string }): Restaurant {
  return {
    slug: partial.name.toLowerCase().replace(/\s+/g, '-'),
    description: '',
    themeConfig: {} as Restaurant['themeConfig'],
    isActive: true,
    ownerId: 'o1',
    createdAt: '2026-01-01T00:00:00Z',
    ...partial,
  };
}

// Monday 2026-08-03 12:00 local.
const NOW = new Date(2026, 7, 3, 12, 0, 0);
const OPEN_ALL_DAY: BusinessHour[] = [hour(1, '09:00:00', '23:00:00')];
const CLOSED_MONDAY: BusinessHour[] = [{ dayOfWeek: 1, openTime: '09:00:00', closeTime: '23:00:00', isClosed: true }];

function sampleList(): Restaurant[] {
  return [
    makeRestaurant({
      id: 'a',
      name: 'Alta Cocina',
      averageRating: 4.8,
      deliveryFee: 5,
      estimatedPrepTimeMinutes: 40,
      businessHours: OPEN_ALL_DAY,
    }),
    makeRestaurant({
      id: 'b',
      name: 'Burger Rápida',
      averageRating: 3.2,
      deliveryFee: 2,
      estimatedPrepTimeMinutes: 15,
      businessHours: OPEN_ALL_DAY,
    }),
    makeRestaurant({
      id: 'c',
      name: 'Café de la Plaza',
      averageRating: 4.5,
      deliveryFee: 4,
      estimatedPrepTimeMinutes: 25,
      businessHours: CLOSED_MONDAY,
    }),
    makeRestaurant({
      id: 'd',
      name: 'Deli Sin Datos',
      // no rating/fees/hours — must sort last and never appear in "open now"
    }),
  ];
}

describe('applyBrowseFilters — search', () => {
  it('filters by name and description', () => {
    const result = applyBrowseFilters(sampleList(), { query: 'burger' });
    expect(result.map(r => r.id)).toEqual(['b']);
  });

  it('matches descriptions too', () => {
    const list = sampleList();
    list[0].description = 'pastas artesanales italianas';
    const result = applyBrowseFilters(list, { query: 'pastas' });
    expect(result.map(r => r.id)).toEqual(['a']);
  });

  it('empty query returns everything (original order)', () => {
    const result = applyBrowseFilters(sampleList(), { query: '' });
    expect(result.map(r => r.id)).toEqual(['a', 'b', 'c', 'd']);
  });
});

describe('applyBrowseFilters — sorting', () => {
  it('sorts by rating descending', () => {
    const result = applyBrowseFilters(sampleList(), { sort: 'rating' });
    expect(result.map(r => r.id)).toEqual(['a', 'c', 'b', 'd']);
  });

  it('sorts by prep time ascending (fastest first)', () => {
    const result = applyBrowseFilters(sampleList(), { sort: 'fastest' });
    expect(result.map(r => r.id)).toEqual(['b', 'c', 'a', 'd']);
  });

  it('sorts by delivery fee ascending (cheapest first)', () => {
    const result = applyBrowseFilters(sampleList(), { sort: 'cheapest' });
    expect(result.map(r => r.id)).toEqual(['b', 'c', 'a', 'd']);
  });

  it('keeps original order for relevance', () => {
    const result = applyBrowseFilters(sampleList(), { sort: 'relevance' });
    expect(result.map(r => r.id)).toEqual(['a', 'b', 'c', 'd']);
  });

  it('restaurants without the field always sort last', () => {
    const result = applyBrowseFilters(sampleList(), { sort: 'rating' });
    expect(result[result.length - 1].id).toBe('d');
  });
});

describe('applyBrowseFilters — open now', () => {
  it('keeps only restaurants open at the given time', () => {
    const result = applyBrowseFilters(sampleList(), { openNowOnly: true, now: NOW });
    expect(result.map(r => r.id)).toEqual(['a', 'b']);
  });

  it('restaurants without hours data are excluded from open-now', () => {
    const result = applyBrowseFilters(sampleList(), { openNowOnly: true, now: NOW });
    expect(result.map(r => r.id)).not.toContain('d');
  });

  it('honors midnight crossover schedules', () => {
    const overnight = [hour(1, '22:00:00', '02:00:00')];
    const list = [
      makeRestaurant({ id: 'x', name: 'Bar Nocturno', businessHours: overnight }),
      makeRestaurant({ id: 'y', name: 'Café Diurno', businessHours: OPEN_ALL_DAY }),
    ];
    const lateNight = new Date(2026, 7, 4, 1, 30, 0);
    const result = applyBrowseFilters(list, { openNowOnly: true, now: lateNight });
    expect(result.map(r => r.id)).toEqual(['x']);
  });
});

describe('applyBrowseFilters — combined', () => {
  it('applies query, open-now and sort together', () => {
    const result = applyBrowseFilters(sampleList(), {
      query: 'a',
      sort: 'fastest',
      openNowOnly: true,
      now: NOW,
    });
    // query "a" matches "Alta Cocina" and "Burger Rápida"; "Café de la Plaza"
    // is closed on Mondays; fastest first => Burger Rápida, then Alta Cocina.
    expect(result.map(r => r.id)).toEqual(['b', 'a']);
  });
});

describe('hasSortData / hasOpenNowData', () => {
  it('detects when a sort key has data', () => {
    const list = sampleList();
    expect(hasSortData(list, 'rating')).toBe(true);
    expect(hasSortData(list, 'fastest')).toBe(true);
    expect(hasSortData(list, 'cheapest')).toBe(true);
    expect(hasSortData([list[3]], 'rating')).toBe(false);
  });

  it('detects when any restaurant has business hours', () => {
    expect(hasOpenNowData(sampleList())).toBe(true);
    expect(hasOpenNowData([sampleList()[3]])).toBe(false);
  });

  it('types: SortKey is a closed union', () => {
    const keys: SortKey[] = ['relevance', 'rating', 'fastest', 'cheapest'];
    expect(keys).toHaveLength(4);
  });
});
