import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { DashboardService } from '../../core/services/dashboard.service';
import { DataSource, Dashboard, DashboardWidget, SaveWidget, WidgetData, WidgetType } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, ChartModule, ButtonModule, DialogModule, InputTextModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly service = inject(DashboardService);
  dashboards = signal<Dashboard[]>([]);
  dataSources = signal<DataSource[]>([]);
  selectedDashboardId = signal('');
  widgetData = signal<Record<string, WidgetData>>({});
  showDashboardDialog = signal(false);
  showWidgetDialog = signal(false);
  dashboardName = '';
  widgetTitle = '';
  widgetType: WidgetType = 'Chart';
  dataSourceKey = '';
  selectedFields: string[] = [];
  groupBy = '';
  limit = 25;
  loading = signal(true);

  selectedDashboard = computed(() => this.dashboards().find(dashboard => dashboard.id === this.selectedDashboardId()));
  selectedSource = computed(() => this.dataSources().find(source => source.key === this.dataSourceKey));

  ngOnInit(): void {
    this.service.getDataSources().subscribe(sources => this.dataSources.set(sources));
    this.reload();
  }

  reload(selectId?: string): void {
    this.loading.set(true);
    this.service.getAll().subscribe(dashboards => {
      this.dashboards.set(dashboards);
      this.selectedDashboardId.set(selectId ?? this.selectedDashboardId() ?? dashboards[0]?.id ?? '');
      if (!this.selectedDashboardId() && dashboards.length) this.selectedDashboardId.set(dashboards[0].id);
      this.loading.set(false);
      this.loadWidgetData();
    });
  }

  createDashboard(): void {
    if (!this.dashboardName.trim()) return;
    this.service.create(this.dashboardName).subscribe(dashboard => {
      this.dashboardName = '';
      this.showDashboardDialog.set(false);
      this.reload(dashboard.id);
    });
  }

  selectSource(key: string): void {
    this.dataSourceKey = key;
    this.selectedFields = this.selectedSource()?.fields.slice(0, 2).map(field => field.key) ?? [];
    this.groupBy = '';
  }

  toggleField(key: string): void {
    this.selectedFields = this.selectedFields.includes(key) ? this.selectedFields.filter(field => field !== key) : [...this.selectedFields, key];
  }

  addWidget(): void {
    const dashboard = this.selectedDashboard();
    const source = this.selectedSource();
    if (!dashboard || !source || !this.widgetTitle.trim() || !this.selectedFields.length) return;
    this.service.addWidget(dashboard.id, this.widgetRequest()).subscribe(() => {
      this.showWidgetDialog.set(false);
      this.resetWidgetForm();
      this.reload(dashboard.id);
    });
  }

  removeWidget(widget: DashboardWidget): void {
    const dashboard = this.selectedDashboard();
    if (!dashboard) return;
    this.service.deleteWidget(dashboard.id, widget.id).subscribe(() => this.reload(dashboard.id));
  }

  chartData(widget: DashboardWidget) {
    const rows = this.widgetData()[widget.id]?.rows ?? [];
    const label = widget.visualization.labelField ?? Object.keys(rows[0] ?? {})[0];
    const value = widget.visualization.valueField ?? Object.keys(rows[0] ?? {})[1];
    return { labels: rows.map(row => String(row[label] ?? '')), datasets: [{ label: widget.title, data: rows.map(row => Number(row[value] ?? 0)), backgroundColor: '#2f5cff', borderRadius: 8 }] };
  }

  private loadWidgetData(): void {
    const dashboard = this.selectedDashboard();
    if (!dashboard) return;
    dashboard.widgets.forEach(widget => this.service.getWidgetData(dashboard.id, widget.id).subscribe(data => this.widgetData.update(current => ({ ...current, [widget.id]: data }))));
  }

  private widgetRequest(): SaveWidget {
    const aggregation = this.widgetType === 'Chart' ? { function: 'count', alias: 'count' } : undefined;
    return { title: this.widgetTitle, widgetType: this.widgetType, dataSourceKey: this.dataSourceKey, query: { fields: this.selectedFields, filters: [], groupBy: this.widgetType === 'Chart' ? this.groupBy || this.selectedFields[0] : undefined, aggregation, limit: this.limit }, visualization: { chartType: 'bar', labelField: this.groupBy || this.selectedFields[0], valueField: 'count' }, row: 0, column: 0, width: 6, height: 4 };
  }

  private resetWidgetForm(): void {
    this.widgetTitle = '';
    this.dataSourceKey = '';
    this.selectedFields = [];
    this.groupBy = '';
  }
}
