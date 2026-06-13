import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { provideStore, withNgxsDevelopmentOptions } from '@ngxs/store';
import { routes } from './app.routes';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { AuthState } from './features/auth/state/auth.state';
import { PermissionsState } from './features/permissions/state/permissions.state';
import { RolesState } from './features/roles/state/roles.state';
import { TenantsState } from './features/tenants/state/tenants.state';
import { UsersState } from './features/users/state/users.state';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withFetch(), withInterceptors([jwtInterceptor])),
    provideStore(
      [AuthState, UsersState, RolesState, PermissionsState, TenantsState],
      withNgxsDevelopmentOptions({ warnOnUnhandledActions: true })
    ),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.dark',
          cssLayer: {
            name: 'primeng',
            order: 'tailwind-base, primeng, tailwind-utilities'
          }
        }
      }
    })
  ]
};
