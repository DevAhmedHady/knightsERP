import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/dashboard' } as RouterStateSnapshot;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient()
      ]
    });
  });

  afterEach(() => localStorage.clear());

  it('allows authenticated users', () => {
    localStorage.setItem('knights.accessToken', 'test-token');

    const result = TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(result).toBe(true);
  });

  it('redirects unauthenticated users to login', () => {
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
