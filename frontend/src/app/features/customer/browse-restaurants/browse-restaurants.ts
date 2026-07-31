import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RestaurantService, Restaurant } from '../../../core/services/restaurant.service';

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

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.restaurantService.getAll().subscribe({
      next: (data) => {
        this.restaurants = data;
        this.filteredRestaurants = data;
        this.loading = false;
      },
      error: () => {
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
