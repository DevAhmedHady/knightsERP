import { inject, Injectable } from '@angular/core';
import { Action, Selector, State, StateContext } from '@ngxs/store';
import { catchError, tap, throwError } from 'rxjs';
import {
  FeatureCatalogItemResponse,
  TenantResponse,
  TenantSetupSummaryResponse
} from '../../../core/models/tenant.model';
import { TenantService } from '../../../core/services/tenant.service';
import {
  AddTenantMember,
  AssignTenantRole,
  ConfigureCurrentTenantEnvironment,
  CreateFeatureCatalogItem,
  CreateTenant,
  DeactivateTenant,
  GrantTenantPermission,
  LoadCurrentTenantSetup,
  LoadFeatureCatalog,
  LoadTenant,
  LoadTenantByCodeName,
  LoadTenants,
  RemoveCurrentTenantFeature,
  RemoveTenantMember,
  RemoveTenantRole,
  RevokeTenantPermission,
  SelectCurrentTenantFeature,
  UpdateCurrentTenantFeatureSettings,
  UpdateFeatureCatalogItem,
  UpdateTenant
} from './tenants.actions';

export interface TenantsStateModel {
  items: TenantResponse[];
  selected?: TenantResponse;
  featureCatalog: FeatureCatalogItemResponse[];
  setup?: TenantSetupSummaryResponse;
  loading: boolean;
  error?: string;
}

@State<TenantsStateModel>({
  name: 'tenants',
  defaults: { items: [], featureCatalog: [], loading: false }
})
@Injectable()
export class TenantsState {
  private readonly tenantService = inject(TenantService);

  @Selector()
  static items(state: TenantsStateModel): TenantResponse[] { return state.items; }

  @Selector()
  static selected(state: TenantsStateModel): TenantResponse | undefined { return state.selected; }

  @Selector()
  static featureCatalog(state: TenantsStateModel): FeatureCatalogItemResponse[] { return state.featureCatalog; }

  @Selector()
  static setup(state: TenantsStateModel): TenantSetupSummaryResponse | undefined { return state.setup; }

  @Selector()
  static loading(state: TenantsStateModel): boolean { return state.loading; }

  static byId(id: string) {
    return (state: TenantsStateModel) => state.items.find(tenant => tenant.id === id);
  }

  static byCodeName(codeName: string) {
    return (state: TenantsStateModel) => state.items.find(tenant => tenant.codeName === codeName);
  }

  @Action(LoadTenants)
  loadTenants(ctx: StateContext<TenantsStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.tenantService.getAll().pipe(
      tap(items => ctx.patchState({ items, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadTenant)
  loadTenant(ctx: StateContext<TenantsStateModel>, action: LoadTenant) {
    ctx.patchState({ loading: true, error: undefined });
    return this.tenantService.getById(action.id).pipe(
      tap(selected => ctx.patchState({ selected, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadTenantByCodeName)
  loadTenantByCodeName(ctx: StateContext<TenantsStateModel>, action: LoadTenantByCodeName) {
    ctx.patchState({ loading: true, error: undefined });
    return this.tenantService.getByCodeName(action.codeName).pipe(
      tap(selected => ctx.patchState({ selected, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(CreateTenant)
  createTenant(ctx: StateContext<TenantsStateModel>, action: CreateTenant) {
    return this.tenantService.create(action.request).pipe(
      tap(tenant => ctx.patchState({ items: [tenant, ...ctx.getState().items], selected: tenant, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdateTenant)
  updateTenant(ctx: StateContext<TenantsStateModel>, action: UpdateTenant) {
    return this.tenantService.update(action.id, action.request).pipe(
      tap(() => this.patchTenant(ctx, action.id, tenant => ({ ...tenant, ...action.request }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(DeactivateTenant)
  deactivateTenant(ctx: StateContext<TenantsStateModel>, action: DeactivateTenant) {
    return this.tenantService.deactivate(action.id).pipe(
      tap(() => this.patchTenant(ctx, action.id, tenant => ({ ...tenant, isActive: false }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(AssignTenantRole)
  assignRole(ctx: StateContext<TenantsStateModel>, action: AssignTenantRole) {
    return this.tenantService.assignRole(action.tenantId, action.roleId).pipe(
      tap(() => this.patchTenant(ctx, action.tenantId, tenant => ({
        ...tenant,
        roleIds: tenant.roleIds.includes(action.roleId) ? tenant.roleIds : [...tenant.roleIds, action.roleId]
      }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(RemoveTenantRole)
  removeRole(ctx: StateContext<TenantsStateModel>, action: RemoveTenantRole) {
    return this.tenantService.removeRole(action.tenantId, action.roleId).pipe(
      tap(() => this.patchTenant(ctx, action.tenantId, tenant => ({
        ...tenant,
        roleIds: tenant.roleIds.filter(roleId => roleId !== action.roleId)
      }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(GrantTenantPermission)
  grantPermission(ctx: StateContext<TenantsStateModel>, action: GrantTenantPermission) {
    return this.tenantService.grantPermission(action.tenantId, action.permissionId).pipe(
      tap(() => this.patchTenant(ctx, action.tenantId, tenant => ({
        ...tenant,
        permissionIds: tenant.permissionIds.includes(action.permissionId)
          ? tenant.permissionIds
          : [...tenant.permissionIds, action.permissionId]
      }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(RevokeTenantPermission)
  revokePermission(ctx: StateContext<TenantsStateModel>, action: RevokeTenantPermission) {
    return this.tenantService.revokePermission(action.tenantId, action.permissionId).pipe(
      tap(() => this.patchTenant(ctx, action.tenantId, tenant => ({
        ...tenant,
        permissionIds: tenant.permissionIds.filter(permissionId => permissionId !== action.permissionId)
      }))),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(AddTenantMember)
  addMember(ctx: StateContext<TenantsStateModel>, action: AddTenantMember) {
    return this.tenantService.addMember(action.tenantId, action.userId).pipe(
      tap(() => ctx.patchState({ error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(RemoveTenantMember)
  removeMember(ctx: StateContext<TenantsStateModel>, action: RemoveTenantMember) {
    return this.tenantService.removeMember(action.tenantId, action.userId).pipe(
      tap(() => ctx.patchState({ error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadCurrentTenantSetup)
  loadCurrentSetup(ctx: StateContext<TenantsStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.tenantService.getCurrentSetup().pipe(
      tap(setup => ctx.patchState({ setup, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(ConfigureCurrentTenantEnvironment)
  configureCurrentEnvironment(ctx: StateContext<TenantsStateModel>, action: ConfigureCurrentTenantEnvironment) {
    return this.tenantService.configureCurrentEnvironment(action.request).pipe(
      tap(setup => ctx.patchState({ setup, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(SelectCurrentTenantFeature)
  selectCurrentFeature(ctx: StateContext<TenantsStateModel>, action: SelectCurrentTenantFeature) {
    return this.tenantService.selectCurrentFeature(action.featureId).pipe(
      tap(setup => ctx.patchState({ setup, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(RemoveCurrentTenantFeature)
  removeCurrentFeature(ctx: StateContext<TenantsStateModel>, action: RemoveCurrentTenantFeature) {
    return this.tenantService.removeCurrentFeature(action.featureId).pipe(
      tap(setup => ctx.patchState({ setup, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdateCurrentTenantFeatureSettings)
  updateCurrentFeatureSettings(ctx: StateContext<TenantsStateModel>, action: UpdateCurrentTenantFeatureSettings) {
    return this.tenantService.updateCurrentFeatureSettings(action.featureId, action.settingsJson).pipe(
      tap(setup => ctx.patchState({ setup, error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(LoadFeatureCatalog)
  loadFeatureCatalog(ctx: StateContext<TenantsStateModel>) {
    ctx.patchState({ loading: true, error: undefined });
    return this.tenantService.getFeatureCatalog().pipe(
      tap(featureCatalog => ctx.patchState({ featureCatalog, loading: false })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(CreateFeatureCatalogItem)
  createFeatureCatalogItem(ctx: StateContext<TenantsStateModel>, action: CreateFeatureCatalogItem) {
    return this.tenantService.createFeatureCatalogItem(action.request).pipe(
      tap(item => ctx.patchState({ featureCatalog: [item, ...ctx.getState().featureCatalog], error: undefined })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  @Action(UpdateFeatureCatalogItem)
  updateFeatureCatalogItem(ctx: StateContext<TenantsStateModel>, action: UpdateFeatureCatalogItem) {
    return this.tenantService.updateFeatureCatalogItem(action.featureId, action.request).pipe(
      tap(item => ctx.patchState({
        featureCatalog: ctx.getState().featureCatalog.map(feature => feature.id === item.id ? item : feature),
        error: undefined
      })),
      catchError(error => this.recordFailure(ctx, error))
    );
  }

  private patchTenant(
    ctx: StateContext<TenantsStateModel>,
    tenantId: string,
    update: (tenant: TenantResponse) => TenantResponse
  ): void {
    const state = ctx.getState();
    ctx.patchState({
      items: state.items.map(tenant => tenant.id === tenantId ? update(tenant) : tenant),
      selected: state.selected?.id === tenantId ? update(state.selected) : state.selected,
      error: undefined
    });
  }

  private recordFailure(ctx: StateContext<TenantsStateModel>, error: unknown) {
    ctx.patchState({ loading: false, error: 'Tenant operation failed.' });
    return throwError(() => error);
  }
}
