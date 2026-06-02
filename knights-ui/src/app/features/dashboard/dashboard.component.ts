import { Component, computed, inject, OnInit, signal } from '@angular/core';
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
  coverageScore = computed(() => {
    const total = this.totalRoles() + this.totalPermissions();
    if (total === 0) {
      return 0;
    }

    return Math.round(((this.activeRoles() * 2) + this.totalPermissions()) / (total * 2) * 100);
  });
  inactiveRoles = computed(() => Math.max(0, this.totalRoles() - this.activeRoles()));

  chartData = signal<any>(null);
  chartOptions = signal<any>({
    responsive: true,
    maintainAspectRatio: true,
    cutout: '72%',
    plugins: {
      legend: { display: false },
      tooltip: { callbacks: { label: (ctx: any) => ` ${ctx.parsed} roles` } }
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

        const active = roles.filter(r => r.isActive).length;
        const inactive = roles.length - active;
        this.chartData.set({
          labels: ['Active', 'Inactive'],
          datasets: [{
            data: [active, inactive],
            backgroundColor: ['#0f7d5c', '#d97706'],
            hoverBackgroundColor: ['#138a61', '#b45309'],
            borderWidth: 0
          }]
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
