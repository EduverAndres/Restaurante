import { Component, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { CartDrawer } from '../../core/components/cart-drawer/cart-drawer';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, CartDrawer],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  isMobileMenuOpen = false;
  isScrolled = false;
  isCartOpen = false;

  constructor(protected auth: AuthService, protected cart: CartService) {}

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled = window.scrollY > 20;
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  openCart(): void {
    this.isCartOpen = true;
  }

  closeCart(): void {
    this.isCartOpen = false;
  }

  onLogout(): void {
    this.closeMobileMenu();
    this.auth.logout();
  }
}
