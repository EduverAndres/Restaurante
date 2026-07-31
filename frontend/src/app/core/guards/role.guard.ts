import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: ('customer' | 'restaurant')[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.isLoggedIn() && auth.userRole() && allowedRoles.includes(auth.userRole()!)) {
      return true;
    }

    router.navigate(['/']);
    return false;
  };
};
