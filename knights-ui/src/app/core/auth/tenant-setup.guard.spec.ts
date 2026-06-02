import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot } from '@angular/router';
import { firstValueFrom, of, throwError } from 'rxjs';
import { tenantSetupGuard } from './tenant-setup.guard';
import { AuthService } from '../services/auth.service';
import { TenantService } from '../services/tenant.service';

describe('tenantSetupGuard', () => {
  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/dashboard' } as RouterStateSnapshot;

  const tenantService = {
    getCurrentSetup: jasmine.createSpy()
  };

  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('knights.accessToken', 'token');
    localStorage.setItem('knights.tenantId', 'tenant-1');

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        AuthService,
        { provide: TenantService, useValue: tenantService }
      ]
    });
  });

  afterEach(() => {
    localStorage.clear();
    tenantService.getCurrentSetup.calls.reset();
  });

  it('allows tenant access when setup is unlocked', async () => {
    tenantService.getCurrentSetup.and.returnValue(of({ isUnlocked: true }));

    const result = TestBed.runInInjectionContext(() => tenantSetupGuard(route, state));

    await expectAsync(firstValueFrom(result as never)).toBeResolvedTo(true);
  });

  it('redirects locked tenants to setup', async () => {
    const router = TestBed.inject(Router);
    tenantService.getCurrentSetup.and.returnValue(of({ isUnlocked: false }));

    const result = TestBed.runInInjectionContext(() => tenantSetupGuard(route, state));

    await expectAsync(firstValueFrom(result as never)).toBeResolvedTo(router.createUrlTree(['/setup'], { queryParams: { redirect: '/dashboard' } }));
  });

  it('passes through on setup lookup error', async () => {
    tenantService.getCurrentSetup.and.returnValue(throwError(() => new Error('network')));

    const result = TestBed.runInInjectionContext(() => tenantSetupGuard(route, state));

    await expectAsync(firstValueFrom(result as never)).toBeResolvedTo(true);
  });
});
