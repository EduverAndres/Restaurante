import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { Landing } from './features/landing/landing';
import { Login } from './features/auth/login';
import { Register } from './features/auth/register';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'auth/login', component: Login },
  { path: 'auth/register', component: Register },
  {
    path: 'browse',
    loadComponent: () => import('./features/customer/browse-restaurants/browse-restaurants').then(m => m.BrowseRestaurants),
    canActivate: [authGuard],
  },
  {
    path: 'restaurant/:slug',
    loadComponent: () => import('./features/customer/restaurant-view/restaurant-view').then(m => m.RestaurantView),
  },
  {
    path: 'restaurant/:slug/order/ai',
    loadComponent: () => import('./features/customer/ai-assistant/ai-assistant').then(m => m.AiAssistant),
    canActivate: [authGuard],
  },
  {
    path: 'customer/orders',
    loadComponent: () => import('./features/customer/my-orders/customer-orders').then(m => m.CustomerOrders),
    canActivate: [roleGuard(['customer'])],
  },
  {
    path: 'customer/orders/:id',
    loadComponent: () => import('./features/customer/my-orders/order-detail').then(m => m.OrderDetail),
    canActivate: [roleGuard(['customer'])],
  },
  {
    path: 'checkout',
    loadComponent: () => import('./features/customer/checkout/checkout').then(m => m.Checkout),
    canActivate: [() => authGuard('/checkout'), roleGuard(['customer'])],
  },
  {
    path: 'restaurant/dashboard',
    loadComponent: () => import('./features/restaurant/dashboard/dashboard').then(m => m.Dashboard),
    canActivate: [roleGuard(['restaurant'])],
  },
  {
    path: 'restaurant/menu',
    loadComponent: () => import('./features/restaurant/menu-manager/menu-manager').then(m => m.MenuManager),
    canActivate: [roleGuard(['restaurant'])],
  },
  {
    path: 'restaurant/orders',
    loadComponent: () => import('./features/restaurant/order-manager/restaurant-orders').then(m => m.RestaurantOrders),
    canActivate: [roleGuard(['restaurant'])],
  },
  {
    path: 'restaurant/storefront',
    loadComponent: () => import('./features/restaurant/storefront-editor/storefront-editor').then(m => m.StorefrontEditor),
    canActivate: [roleGuard(['restaurant'])],
  },
  {
    path: 'restaurant/profile',
    loadComponent: () => import('./features/restaurant/restaurant-profile/restaurant-profile').then(m => m.RestaurantProfile),
    canActivate: [roleGuard(['restaurant'])],
  },
  {
    path: 'restaurant/posts',
    loadComponent: () => import('./features/restaurant/restaurant-posts/restaurant-posts').then(m => m.RestaurantPosts),
    canActivate: [roleGuard(['restaurant'])],
  },
  { path: '**', redirectTo: '' },
];
