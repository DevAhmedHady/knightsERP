import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { authGuard } from './core/auth/auth.guard';
import { tenantSetupGuard } from './core/auth/tenant-setup.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'setup',
        loadComponent: () => import('./features/tenants/tenant-setup/tenant-setup.component').then(m => m.TenantSetupComponent)
      },
      {
        path: 'dashboard',
        canActivate: [tenantSetupGuard],
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'tenants',
        canActivate: [tenantSetupGuard],
        loadComponent: () => import('./features/tenants/tenants-list/tenants-list.component').then(m => m.TenantsListComponent)
      },
      {
        path: 'feature-catalog',
        loadComponent: () => import('./features/tenants/feature-catalog/feature-catalog.component').then(m => m.FeatureCatalogComponent)
      },
      {
        path: 'users',
        canActivate: [tenantSetupGuard],
        loadComponent: () => import('./features/users/users-list/users-list.component').then(m => m.UsersListComponent)
      },
      {
        path: 'roles',
        canActivate: [tenantSetupGuard],
        loadComponent: () => import('./features/roles/roles-list/roles-list.component').then(m => m.RolesListComponent)
      },
      {
        path: 'permissions',
        canActivate: [tenantSetupGuard],
        loadComponent: () => import('./features/permissions/permissions-list/permissions-list.component').then(m => m.PermissionsListComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
