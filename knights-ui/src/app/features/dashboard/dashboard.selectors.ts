import { createSelector } from '@ngxs/store';
import { PermissionsState, PermissionsStateModel } from '../permissions/state/permissions.state';
import { RolesState, RolesStateModel } from '../roles/state/roles.state';

export interface DashboardMetrics {
  totalRoles: number;
  activeRoles: number;
  totalPermissions: number;
  loading: boolean;
}

export const dashboardMetrics = createSelector(
  [RolesState, PermissionsState],
  (roles: RolesStateModel, permissions: PermissionsStateModel): DashboardMetrics => ({
    totalRoles: roles.items.length,
    activeRoles: roles.items.filter(role => role.isActive).length,
    totalPermissions: permissions.items.length,
    loading: roles.loading || permissions.loading
  })
);
