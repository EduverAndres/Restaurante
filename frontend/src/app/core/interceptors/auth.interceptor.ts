import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { from } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse && error.status === 401 && token) {
        return auth.refreshToken().pipe(
          switchMap(res => {
            const cloned = req.clone({
              setHeaders: { Authorization: `Bearer ${res.token}` },
            });
            return next(cloned);
          }),
          catchError(() => {
            auth.logout();
            return throwError(() => error);
          }),
        );
      }
      return throwError(() => error);
    }),
  );
};
