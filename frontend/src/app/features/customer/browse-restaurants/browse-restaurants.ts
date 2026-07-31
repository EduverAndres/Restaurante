import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';
import { SAMPLE_RESTAURANTS } from '../../../core/data/sample-restaurants';

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
  loading = true;
  /** true mientras se muestran los restaurantes de ejemplo (sin datos reales todavía) */
  usingSampleData = false;

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.restaurantService.getAll().subscribe({
      next: (data) => {
        if (data.length > 0) {
          this.restaurants = data;
        } else {
          this.restaurants = SAMPLE_RESTAURANTS;
          this.usingSampleData = true;
        }
        this.filteredRestaurants = this.restaurants;
        this.loading = false;
      },
      error: () => {
        this.restaurants = SAMPLE_RESTAURANTS;
        this.filteredRestaurants = this.restaurants;
        this.usingSampleData = true;
        this.loading = false;
      },
    });
  }

  onSearch(): void {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) {
      this.filteredRestaurants = this.restaurants;
      return;
    }
    this.filteredRestaurants = this.restaurants.filter(
      r => r.name.toLowerCase().includes(q) || r.description.toLowerCase().includes(q),
    );
  }
}