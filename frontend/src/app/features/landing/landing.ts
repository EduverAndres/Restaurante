import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RestaurantService, Restaurant } from '../../core/services/restaurant.service';

@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing implements OnInit, OnDestroy {
  featuredRestaurants: Restaurant[] = [];
  private observer?: IntersectionObserver;

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.restaurantService.getAll().subscribe({
      next: (restaurants) => {
        this.featuredRestaurants = restaurants.slice(0, 6);
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
