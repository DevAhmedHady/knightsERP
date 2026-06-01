import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  pending = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    userNameOrEmail: ['', Validators.required],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid || this.pending()) return;

    this.pending.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();

    this.authService.login({
      userNameOrEmail: value.userNameOrEmail!,
      password: value.password!
    }).pipe(
      finalize(() => this.pending.set(false))
    ).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err: HttpErrorResponse) => {
        this.error.set(err.status === 401 ? 'Invalid username or password.' : 'Sign in failed.');
      }
    });
  }
}
