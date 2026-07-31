import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RestaurantService, Restaurant } from '../../core/services/restaurant.service';
import { SAMPLE_RESTAURANTS } from '../../core/data/sample-restaurants';

@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing implements OnInit, OnDestroy {
  featuredRestaurants: Restaurant[] = [];
  /** true mientras se muestran los restaurantes de ejemplo (sin datos reales todavía) */
  usingSampleData = false;
  private observer?: IntersectionObserver;

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.restaurantService.getAll().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.featuredRestaurants = restaurants.slice(0, 6);
        } else {
          this.featuredRestaurants = SAMPLE_RESTAURANTS.slice(0, 6);
          this.usingSampleData = true;
        }
      },
      error: () => {
        this.featuredRestaurants = SAMPLE_RESTAURANTS.slice(0, 6);
        this.usingSampleData = true;
      },
    });

    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.1 }
    );

    setTimeout(() => {
      document.querySelectorAll('.reveal').forEach((el) => {
        this.observer?.observe(el);
      });
    }, 100);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}