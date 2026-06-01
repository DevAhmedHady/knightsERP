import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { authGuard } from './core/auth/auth.guard';

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
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'tenants',
        loadComponent: () => import('./features/tenants/tenants-list/tenants-list.component').then(m => m.TenantsListComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users-list/users-list.component').then(m => m.UsersListComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/roles/roles-list/roles-list.component').then(m => m.RolesListComponent)
      },
      {
        path: 'permissions',
        loadComponent: () => import('./features/permissions/permissions-list/permissions-list.component').then(m => m.PermissionsListComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
