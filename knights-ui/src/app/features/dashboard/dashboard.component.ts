import { Component, inject, OnInit, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { ChartModule } from 'primeng/chart';
import { RoleService } from '../../core/services/role.service';
import { PermissionService } from '../../core/services/permission.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CardModule, SkeletonModule, ChartModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private roleService = inject(RoleService);
  private permissionService = inject(PermissionService);

  loading = signal(true);
  totalRoles = signal(0);
  activeRoles = signal(0);
  totalPermissions = signal(0);

  chartData = signal<any>(null);
  chartOptions = signal<any>({
    responsive: true,
    plugins: {
      legend: { position: 'bottom' }
    }
  });

  ngOnInit(): void {
    forkJoin({
      roles: this.roleService.getAll(),
      permissions: this.permissionService.getAll()
    }).subscribe({
      next: ({ roles, permissions }) => {
        this.totalRoles.set(roles.length);
        this.activeRoles.set(roles.filter(r => r.isActive).length);
        this.totalPermissions.set(permissions.length);

        this.chartData.set({
          labels: permissions.slice(0, 8).map(p => p.codeName),
          datasets: [{
            label: 'Permissions',
            data: permissions.slice(0, 8).map(() => Math.floor(Math.random() * 10) + 1),
            backgroundColor: [
              '#0d67b2', '#138a61', '#d97706', '#c2410c',
              '#334155', '#1683d8', '#0d6f50', '#a16207'
            ]
          }]
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
