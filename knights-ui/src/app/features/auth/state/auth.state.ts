import { inject, Injectable } from '@angular/core';
import { Action, Selector, State, StateContext } from '@ngxs/store';
import { catchError, tap, throwError } from 'rxjs';
import { UserResponse } from '../../../core/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
import { Login, Logout, RestoreSession } from './auth.actions';

export interface AuthStateModel {
  token: string | null;
  currentUser: UserResponse | null;
  tenantId: string | null;
  loading: boolean;
  error?: string;
}

@State<AuthStateModel>({
  name: 'auth',
  defaults: { token: null, currentUser: null, tenantId: null, loading: false }
})
@Injectable()
export class AuthState {
  private readonly authService = inject(AuthService);

  @Selector()
  static token(state: AuthStateModel): string | null { return state.token; }

  @Selector()
  static currentUser(state: AuthStateModel): UserResponse | null { return state.currentUser; }

  @Selector()
  static isSystemAdmin(state: AuthStateModel): boolean { return !state.tenantId; }

  @Selector()
  static loading(state: AuthStateModel): boolean { return state.loading; }

  @Selector()
  static error(state: AuthStateModel): string | undefined { return state.error; }

  @Action(Login)
  login(ctx: StateContext<AuthStateModel>, action: Login) {
    ctx.patchState({ loading: true, error: undefined });
    return this.authService.login(action.request).pipe(
      tap(response => ctx.patchState({
        token: response.accessToken,
        currentUser: response.user,
        tenantId: response.tenantId ?? null,
        loading: false
      })),
      catchError(error => {
        ctx.patchState({ loading: false, error: error?.status === 401 ? 'Invalid credentials or tenant code.' : 'Sign in failed.' });
        return throwError(() => error);
      })
    );
  }

  @Action(Logout)
  logout(ctx: StateContext<AuthStateModel>): void {
    this.authService.logout();
    ctx.setState({ token: null, currentUser: null, tenantId: null, loading: false });
  }

  @Action(RestoreSession)
  restoreSession(ctx: StateContext<AuthStateModel>): void {
    ctx.patchState({
      token: this.authService.token,
      currentUser: this.authService.currentUser,
      tenantId: this.authService.tenantId,
      error: undefined
    });
  }
}
