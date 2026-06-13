import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { select, Store } from '@ngxs/store';
import { Login } from '../state/auth.actions';
import { AuthState } from '../state/auth.state';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);

  pending = select(AuthState.loading);
  error = select(AuthState.error);

  form = this.fb.group({
    userNameOrEmail: ['', Validators.required],
    password: ['', Validators.required],
    tenantCodeName: ['']
  });

  submit(): void {
    if (this.form.invalid || this.pending()) return;

    const value = this.form.getRawValue();
    const tenantCode = value.tenantCodeName?.trim() || undefined;

    this.store.dispatch(new Login({
      userNameOrEmail: value.userNameOrEmail!,
      password: value.password!,
      tenantCodeName: tenantCode
    })).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => undefined
    });
  }
}
