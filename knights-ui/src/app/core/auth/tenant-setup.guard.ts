import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TenantService } from '../services/tenant.service';

export const tenantSetupGuard: CanActivateFn = (_, state) => {
  const authService = inject(AuthService);
  const tenantService = inject(TenantService);
  const router = inject(Router);

  if (!authService.isAuthenticated() || authService.isSystemAdmin) {
    return true;
  }

  return tenantService.getCurrentSetup().pipe(
    map(summary => summary.isUnlocked ? true : router.createUrlTree(['/setup'], { queryParams: { redirect: state.url } })),
    catchError(() => of(true))
  );
};
