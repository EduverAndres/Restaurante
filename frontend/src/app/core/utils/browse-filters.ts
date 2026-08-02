import { Restaurant } from '../services/restaurant.service';
import { hasBusinessHours, isOpenNow } from './business-hours';

export type SortKey = 'rating' | 'fastest' | 'cheapest' | 'relevance';

export interface BrowseFilterOptions {
  query?: string;
  sort?: SortKey;
  openNowOnly?: boolean;
  now?: Date;
}

export const SORT_LABELS: Record<SortKey, string> = {
  relevance: 'Relevancia',
  rating: 'Mejor valorados',
  fastest: 'Entrega más rápida',
  cheapest: 'Envío más barato',
};

const SORT_FIELD: Record<SortKey, keyof Restaurant | null> = {
  relevance: null,
  rating: 'averageRating',
  fastest: 'estimatedPrepTimeMinutes',
  cheapest: 'deliveryFee',
};

export function hasSortData(restaurants: Restaurant[], key: Exclude<SortKey, 'relevance'>): boolean {
  const field = SORT_FIELD[key];
  if (!field) return false;
  return restaurants.some(r => typeof (r as any)[field] === 'number' && !Number.isNaN((r as any)[field]));
}

export function hasOpenNowData(restaurants: Restaurant[]): boolean {
  return restaurants.some(r => hasBusinessHours(r.businessHours));
}

function matchesQuery(restaurant: Restaurant, query: string): boolean {
  const q = query.trim().toLowerCase();
  if (!q) return true;
  return (
    restaurant.name.toLowerCase().includes(q) ||
    (restaurant.description || '').toLowerCase().includes(q)
  );
}

function compareRestaurants(a: Restaurant, b: Restaurant, sort: SortKey): number {
  const field = SORT_FIELD[sort];
  if (!field) return 0;
  const va = (a as any)[field];
  const vb = (b as any)[field];
  if (va === undefined && vb === undefined) return 0;
  if (va === undefined) return 1; // missing values sort last
  if (vb === undefined) return -1;
  if (sort === 'rating') return vb - va; // desc
  return va - vb; // asc
}

/**
 * Client-side filter + sort for the browse grid. Pure and deterministic:
 * - search by name/description (same as the pre-existing search)
 * - optional "open now" filter (restaurants without hours data are excluded)
 * - optional sort: rating desc | prep time asc | delivery fee asc
 * Missing numeric fields sort last; never throws.
 */
export function applyBrowseFilters(restaurants: Restaurant[], options: BrowseFilterOptions = {}): Restaurant[] {
  const { query = '', sort = 'relevance', openNowOnly = false, now } = options;

  let result = restaurants.filter(r => matchesQuery(r, query));

  if (openNowOnly) {
    result = result.filter(r => isOpenNow(r.businessHours, now) === true);
  }

  if (sort === 'relevance') return result;
  return [...result].sort((a, b) => compareRestaurants(a, b, sort));
}
