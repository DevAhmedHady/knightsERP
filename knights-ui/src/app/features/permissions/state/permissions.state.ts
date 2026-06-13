import { inject, Injectable } from '@angular/core';
import { Action, Selector, State, StateContext } from '@ngxs/store';
import { catchError, tap, throwError } from 'rxjs';
import { PermissionResponse } from '../../../core/models/permission.model';
import { PermissionService } from '../../../core/services/permission.service';
import { CreatePermission, DeletePermission, LoadPermission, LoadPermissions, UpdatePermission } from './permissions.actions';

export interface PermissionsStateModel {
  items: PermissionResponse[];
  selected?: PermissionResponse;
  loading: boolean;
  error?: string;
}

@State<PermissionsStateModel>({
  name: 'permissions',
  defaults: { items: [], loading: false }
})
@Injectable()
export class PermissionsState {
  private readonly permissionService = inject(PermissionService);

  @Selector()
  static items(state: PermissionsStateModel): PermissionResponse[] { return state.items; }

  @Selector()
  static selected(state: PermissionsStateModel): PermissionResponse | undefined { return state.selected; }

  @Selector()
  static loading(state: PermissionsStateModel): boolean { return state.loading; }

  @Selector()
  static count(state: PermissionsStateModel): number { return state.items.length; }

  static byId(id: string) {
    return (state: PermissionsStateModel) => state.items.find(permission => permission.id === id);
  }

  @Action(LoadPermissions)
  loadPermissions(ctx: StateContext<PermissionsStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.permissionService.getAll().pipe(
      tap(items => ctx.patchState({ items, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadPermission)
  loadPermission(ctx: StateContext<PermissionsStateModel>, action: LoadPermission) {
    ctx.patchState({ loading: true, error: undefined });
    return this.permissionService.getById(action.id).pipe(
      tap(selected => ctx.patchState({ selected, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(CreatePermission)
  createPermission(ctx: StateContext<PermissionsStateModel>, action: CreatePermission) {
    return this.permissionService.create(action.request).pipe(
      tap(permission => ctx.patchState({ items: [permission, ...ctx.getState().items], selected: permission, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdatePermission)
  updatePermission(ctx: StateContext<PermissionsStateModel>, action: UpdatePermission) {
    return this.permissionService.update(action.id, action.request).pipe(
      tap(permission => this.upsert(ctx, permission)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(DeletePermission)
  deletePermission(ctx: StateContext<PermissionsStateModel>, action: DeletePermission) {
    return this.permissionService.delete(action.id).pipe(
      tap(() => {
        const state = ctx.getState();
        ctx.patchState({
          items: state.items.filter(permission => permission.id !== action.id),
          selected: state.selected?.id === action.id ? undefined : state.selected,
          error: undefined
        });
      }),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  private upsert(ctx: StateContext<PermissionsStateModel>, permission: PermissionResponse): void {
    const items = ctx.getState().items.map(item => item.id === permission.id ? permission : item);
    ctx.patchState({ items, selected: permission, error: undefined });
  }

  private recordFailure(ctx: StateContext<PermissionsStateModel>, error: unknown) {
    ctx.patchState({ loading: false, error: 'Permission operation failed.' });
    return throwError(() => error);
  }
}
