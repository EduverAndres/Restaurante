import { Component, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of, Observable, catchError, map } from 'rxjs';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';
import { AuthService } from '../../../core/services/auth.service';
import { SAMPLE_RESTAURANTS } from '../../../core/data/sample-restaurants';
import {
  applyBrowseFilters,
  hasSortData,
  hasOpenNowData,
  SortKey,
  SORT_LABELS,
} from '../../../core/utils/browse-filters';
import { isOpenNowLabel } from '../../../core/utils/business-hours';

@Component({
  selector: 'app-browse-restaurants',
  imports: [RouterLink, FormsModule],
  templateUrl: './browse-restaurants.html',
  styleUrl: './browse-restaurants.css',
})
export class BrowseRestaurants implements OnInit {
  restaurants: Restaurant[] = [];
  filteredRestaurants: Restaurant[] = [];
  searchQuery = '';
  sortKey: SortKey = 'relevance';
  openNowOnly = false;
  loading = true;
  /** true mientras se muestran los restaurantes de ejemplo (sin datos reales todavía) */
  usingSampleData = false;

  /** Sort options shown only when the data actually supports them. */
  readonly availableSorts: Exclude<SortKey, 'relevance'>[] = ['rating', 'fastest', 'cheapest'];
  readonly sortLabels = SORT_LABELS;

  constructor(
    private restaurantService: RestaurantService,
    private auth: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    // Redirect restaurant owners to their dashboard
    if (this.auth.isLoggedIn() && this.auth.userRole() === 'restaurant') {
      this.router.navigate(['/restaurant/dashboard']);
      return;
    }

    this.restaurantService.getAll().subscribe({
      next: (data) => {
        if (data.length > 0) {
          // The list DTO is intentionally light (no fees, hours, coords):
          // enrich each restaurant with its detail so the filters can work.
          this.enrich(data).subscribe({
            next: (rich) => {
              this.restaurants = rich;
              this.applyFilters();
              this.loading = false;
            },
            error: () => {
              this.restaurants = SAMPLE_RESTAURANTS;
              this.usingSampleData = true;
              this.applyFilters();
              this.loading = false;
            },
          });
        } else {
          this.restaurants = SAMPLE_RESTAURANTS;
          this.usingSampleData = true;
          this.applyFilters();
          this.loading = false;
        }
      },
      error: () => {
        this.restaurants = SAMPLE_RESTAURANTS;
        this.usingSampleData = true;
        this.applyFilters();
        this.loading = false;
      },
    });
  }

  /** Fetch detail for each list item, merging extra fields. Tolerant: failures keep the list entry. */
  private enrich(list: Restaurant[]): Observable<Restaurant[]> {
    const requests = list.map((r) =>
      this.restaurantService.getById(r.id).pipe(
        map((detail) => ({ ...r, ...detail })),
        catchError(() => of(r)),
      ),
    );
    return forkJoin(requests);
  }

  showSort(key: Exclude<SortKey, 'relevance'>): boolean {
    return hasSortData(this.restaurants, key);
  }

  get showOpenNowFilter(): boolean {
    return hasOpenNowData(this.restaurants);
  }

  applyFilters(): void {
    this.filteredRestaurants = applyBrowseFilters(this.restaurants, {
      query: this.searchQuery,
      sort: this.sortKey,
      openNowOnly: this.openNowOnly,
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  onSort(key: SortKey): void {
    this.sortKey = this.sortKey === key ? 'relevance' : key;
    this.applyFilters();
  }

  toggleOpenNow(): void {
    this.openNowOnly = !this.openNowOnly;
    this.applyFilters();
  }

  openNowLabel(restaurant: Restaurant): string {
    return isOpenNowLabel(restaurant.businessHours);
  }
}
