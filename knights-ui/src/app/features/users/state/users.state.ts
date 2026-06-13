import { inject, Injectable } from '@angular/core';
import { Action, Selector, State, StateContext } from '@ngxs/store';
import { catchError, tap, throwError } from 'rxjs';
import { UserResponse } from '../../../core/models/user.model';
import { UserService } from '../../../core/services/user.service';
import { AssignRole, CreateUser, DeleteUser, GrantPermission, LoadUser, LoadUsers, UpdateUser } from './users.actions';

export interface UsersStateModel {
  items: UserResponse[];
  selected?: UserResponse;
  loading: boolean;
  error?: string;
}

@State<UsersStateModel>({
  name: 'users',
  defaults: { items: [], loading: false }
})
@Injectable()
export class UsersState {
  private readonly userService = inject(UserService);

  @Selector()
  static items(state: UsersStateModel): UserResponse[] { return state.items; }

  @Selector()
  static selected(state: UsersStateModel): UserResponse | undefined { return state.selected; }

  @Selector()
  static loading(state: UsersStateModel): boolean { return state.loading; }

  static byId(id: string) {
    return (state: UsersStateModel) => state.items.find(user => user.id === id);
  }

  @Action(LoadUsers)
  loadUsers(ctx: StateContext<UsersStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.userService.getAll().pipe(
      tap(items => ctx.patchState({ items, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadUser)
  loadUser(ctx: StateContext<UsersStateModel>, action: LoadUser) {
    ctx.patchState({ loading: true, error: undefined });
    return this.userService.getById(action.id).pipe(
      tap(selected => ctx.patchState({ selected, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(CreateUser)
  createUser(ctx: StateContext<UsersStateModel>, action: CreateUser) {
    return this.userService.create(action.request).pipe(
      tap(user => ctx.patchState({ items: [user, ...ctx.getState().items], selected: user, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdateUser)
  updateUser(ctx: StateContext<UsersStateModel>, action: UpdateUser) {
    return this.userService.update(action.id, action.request).pipe(
      tap(user => this.upsert(ctx, user)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(DeleteUser)
  deleteUser(ctx: StateContext<UsersStateModel>, action: DeleteUser) {
    return this.userService.delete(action.id).pipe(
      tap(() => {
        const state = ctx.getState();
        ctx.patchState({
          items: state.items.filter(user => user.id !== action.id),
          selected: state.selected?.id === action.id ? undefined : state.selected,
          error: undefined
        });
      }),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(AssignRole)
  assignRole(ctx: StateContext<UsersStateModel>, action: AssignRole) {
    return this.userService.assignRole(action.userId, action.roleId).pipe(
      tap(user => this.upsert(ctx, user)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(GrantPermission)
  grantPermission(ctx: StateContext<UsersStateModel>, action: GrantPermission) {
    return this.userService.grantPermission(action.userId, action.permissionId).pipe(
      tap(user => this.upsert(ctx, user)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  private upsert(ctx: StateContext<UsersStateModel>, user: UserResponse): void {
    const items = ctx.getState().items.map(item => item.id === user.id ? user : item);
    ctx.patchState({ items, selected: user, error: undefined });
  }

  private recordFailure(ctx: StateContext<UsersStateModel>, error: unknown) {
    ctx.patchState({ loading: false, error: 'User operation failed.' });
    return throwError(() => error);
  }
}
