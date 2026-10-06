import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

export function demoReadOnly<T>(): Observable<T> {
  return throwError(
    () =>
      new HttpErrorResponse({
        error: 'This public sample is read-only. Run TrueLine locally to sign in and publish stories.',
        status: 403,
      }),
  );
}
