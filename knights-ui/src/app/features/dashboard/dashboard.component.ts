import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { ChartModule } from 'primeng/chart';
import { select, Store } from '@ngxs/store';
import { LoadPermissions } from '../permissions/state/permissions.actions';
import { LoadRoles } from '../roles/state/roles.actions';
import { dashboardMetrics } from './dashboard.selectors';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CardModule, SkeletonModule, ChartModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private store = inject(Store);

  metrics = select(dashboardMetrics);
  loading = computed(() => this.metrics().loading);
  totalRoles = computed(() => this.metrics().totalRoles);
  activeRoles = computed(() => this.metrics().activeRoles);
  totalPermissions = computed(() => this.metrics().totalPermissions);
  coverageScore = computed(() => {
    const total = this.totalRoles() + this.totalPermissions();
    if (total === 0) {
      return 0;
    }

    return Math.round(((this.activeRoles() * 2) + this.totalPermissions()) / (total * 2) * 100);
  });
  inactiveRoles = computed(() => Math.max(0, this.totalRoles() - this.activeRoles()));

  chartData = computed(() => ({
    labels: ['Active', 'Inactive'],
    datasets: [{
      data: [this.activeRoles(), this.inactiveRoles()],
      backgroundColor: ['#0f7d5c', '#d97706'],
      hoverBackgroundColor: ['#138a61', '#b45309'],
      borderWidth: 0
    }]
  }));
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
    this.store.dispatch([new LoadRoles(), new LoadPermissions()]);
  }
}
