import { inject, Injectable } from '@angular/core';
import { Action, Selector, State, StateContext } from '@ngxs/store';
import { catchError, tap, throwError } from 'rxjs';
import { RoleResponse, RoleWithPermissionsResponse } from '../../../core/models/role.model';
import { RoleService } from '../../../core/services/role.service';
import {
  AssignRolePermission,
  CreateRole,
  DeleteRole,
  LoadRole,
  LoadRoles,
  RemoveRolePermission,
  UpdateRole
} from './roles.actions';

export interface RolesStateModel {
  items: RoleResponse[];
  selected?: RoleWithPermissionsResponse;
  loading: boolean;
  error?: string;
}

@State<RolesStateModel>({
  name: 'roles',
  defaults: { items: [], loading: false }
})
@Injectable()
export class RolesState {
  private readonly roleService = inject(RoleService);

  @Selector()
  static items(state: RolesStateModel): RoleResponse[] { return state.items; }

  @Selector()
  static selected(state: RolesStateModel): RoleWithPermissionsResponse | undefined { return state.selected; }

  @Selector()
  static loading(state: RolesStateModel): boolean { return state.loading; }

  @Selector()
  static count(state: RolesStateModel): number { return state.items.length; }

  @Selector()
  static activeCount(state: RolesStateModel): number { return state.items.filter(role => role.isActive).length; }

  static byId(id: string) {
    return (state: RolesStateModel) => state.items.find(role => role.id === id);
  }

  @Action(LoadRoles)
  loadRoles(ctx: StateContext<RolesStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.roleService.getAll().pipe(
      tap(items => ctx.patchState({ items, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadRole)
  loadRole(ctx: StateContext<RolesStateModel>, action: LoadRole) {
    ctx.patchState({ loading: true, error: undefined });
    return this.roleService.getById(action.id).pipe(
      tap(role => ctx.patchState({ selected: role as RoleWithPermissionsResponse, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(CreateRole)
  createRole(ctx: StateContext<RolesStateModel>, action: CreateRole) {
    return this.roleService.create(action.request).pipe(
      tap(role => ctx.patchState({ items: [role, ...ctx.getState().items], error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdateRole)
  updateRole(ctx: StateContext<RolesStateModel>, action: UpdateRole) {
    return this.roleService.update(action.id, action.request).pipe(
      tap(role => {
        const state = ctx.getState();
        ctx.patchState({
          items: state.items.map(item => item.id === role.id ? role : item),
          selected: state.selected?.id === role.id ? { ...state.selected, ...role } : state.selected,
          error: undefined
        });
      }),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(DeleteRole)
  deleteRole(ctx: StateContext<RolesStateModel>, action: DeleteRole) {
    return this.roleService.delete(action.id).pipe(
      tap(() => {
        const state = ctx.getState();
        ctx.patchState({
          items: state.items.filter(role => role.id !== action.id),
          selected: state.selected?.id === action.id ? undefined : state.selected,
          error: undefined
        });
      }),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(AssignRolePermission)
  assignPermission(ctx: StateContext<RolesStateModel>, action: AssignRolePermission) {
    return this.roleService.assignPermission(action.roleId, action.permissionId).pipe(
      tap(role => this.setSelectedRole(ctx, role)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(RemoveRolePermission)
  removePermission(ctx: StateContext<RolesStateModel>, action: RemoveRolePermission) {
    return this.roleService.removePermission(action.roleId, action.permissionId).pipe(
      tap(role => this.setSelectedRole(ctx, role)),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  private setSelectedRole(ctx: StateContext<RolesStateModel>, role: RoleWithPermissionsResponse): void {
    ctx.patchState({
      selected: role,
      items: ctx.getState().items.map(item => item.id === role.id ? role : item),
      error: undefined
    });
  }

  private recordFailure(ctx: StateContext<RolesStateModel>, error: unknown) {
    ctx.patchState({ loading: false, error: 'Role operation failed.' });
    return throwError(() => error);
  }
}
