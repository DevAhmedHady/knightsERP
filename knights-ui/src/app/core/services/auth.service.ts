import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.model';
import { UserResponse } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private base = environment.apiBaseUrl;
  private readonly KEYS = {
    token: 'knights.accessToken',
    tenantId: 'knights.tenantId',
    tenantCodeName: 'knights.tenantCodeName',
    user: 'knights.user',
    expiresAtUtc: 'knights.expiresAtUtc'
  };

  login(req: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base}/api/auth/login`, req).pipe(
      tap(res => {
        localStorage.setItem(this.KEYS.token, res.accessToken);
        localStorage.setItem(this.KEYS.user, JSON.stringify(res.user));
        localStorage.setItem(this.KEYS.expiresAtUtc, res.expiresAtUtc);
        if (res.tenantId) localStorage.setItem(this.KEYS.tenantId, res.tenantId);
        else localStorage.removeItem(this.KEYS.tenantId);
        if (res.tenantCodeName) localStorage.setItem(this.KEYS.tenantCodeName, res.tenantCodeName);
        else localStorage.removeItem(this.KEYS.tenantCodeName);
      })
    );
  }

  get token(): string | null { return localStorage.getItem(this.KEYS.token); }
  get tenantId(): string | null { return localStorage.getItem(this.KEYS.tenantId); }
  get tenantCodeName(): string | null { return localStorage.getItem(this.KEYS.tenantCodeName); }
  get expiresAtUtc(): string | null { return localStorage.getItem(this.KEYS.expiresAtUtc); }
  get isSystemAdmin(): boolean { return !this.tenantId; }
  get currentUser(): UserResponse | null {
    const raw = localStorage.getItem(this.KEYS.user);
    return raw ? JSON.parse(raw) : null;
  }

  isAuthenticated(): boolean {
    if (!this.token) {
      return false;
    }

    if (!this.expiresAtUtc) {
      return true;
    }

    const expiresAt = Date.parse(this.expiresAtUtc);
    if (Number.isNaN(expiresAt)) {
      return true;
    }

    return expiresAt > Date.now();
  }

  logout(): void {
    Object.values(this.KEYS).forEach(k => localStorage.removeItem(k));
  }

  logoutAndRedirect(): void {
    this.logout();
    void this.router.navigate(['/login']);
  }
}
