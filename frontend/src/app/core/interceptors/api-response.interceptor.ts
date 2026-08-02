import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

export const apiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body && typeof event.body === 'object' && 'data' in event.body) {
        const body = event.body as any;
        // Business validation failures arrive as HTTP 200 with { success: false, message, data: null }.
        // Keep the full envelope so callers can read `message` (the checkout depends on it).
        if (body.success === false) {
          return event;
        }
        return event.clone({ body: body.data });
      }
      return event;
    }),
  );
};
