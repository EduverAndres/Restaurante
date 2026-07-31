import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

export const apiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body && typeof event.body === 'object' && 'data' in event.body) {
        return event.clone({ body: (event.body as any).data });
      }
      return event;
    }),
  );
};
