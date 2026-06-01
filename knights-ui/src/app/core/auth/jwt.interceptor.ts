import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const loginUrl = `${environment.apiBaseUrl}/api/auth/login`;
  const token = authService.token;

  if (!token || !req.url.startsWith(environment.apiBaseUrl) || req.url.startsWith(loginUrl)) {
    return next(req);
  }

  return next(req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  }));
};
