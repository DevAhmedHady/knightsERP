import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { LoginResponse } from '../../../core/models/auth.model';
import { AuthService } from '../../../core/services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let authService: { login: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  const response: LoginResponse = {
    accessToken: 'test-token',
    expiresAtUtc: '2026-06-01T00:00:00Z',
    user: {
      id: 'user-id',
      firstName: 'Test',
      lastName: 'User',
      userName: 'test.user',
      email: 'test@example.com',
      isEmailConfirmed: true
    }
  };

  beforeEach(async () => {
    authService = { login: vi.fn() };
    router = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('navigates to dashboard on success', () => {
    authService.login.mockReturnValue(of(response));
    component.form.setValue({ userNameOrEmail: 'test@example.com', password: 'password' });

    component.submit();

    expect(authService.login).toHaveBeenCalledWith({
      userNameOrEmail: 'test@example.com',
      password: 'password'
    });
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('shows error on 401', () => {
    authService.login.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    component.form.setValue({ userNameOrEmail: 'test@example.com', password: 'bad-password' });

    component.submit();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Invalid username or password.');
  });
});
