import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { DataSource, Dashboard, DashboardWidget, SaveWidget, WidgetData } from '../models/dashboard.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/api/dashboards`;

  getAll() { return this.http.get<Dashboard[]>(this.endpoint); }
  create(name: string) { return this.http.post<Dashboard>(this.endpoint, { name }); }
  addWidget(dashboardId: string, widget: SaveWidget) { return this.http.post<DashboardWidget>(`${this.endpoint}/${dashboardId}/widgets`, widget); }
  deleteWidget(dashboardId: string, widgetId: string) { return this.http.delete<void>(`${this.endpoint}/${dashboardId}/widgets/${widgetId}`); }
  getWidgetData(dashboardId: string, widgetId: string) { return this.http.get<WidgetData>(`${this.endpoint}/${dashboardId}/widgets/${widgetId}/data`); }
  getDataSources() { return this.http.get<DataSource[]>(`${this.endpoint}/datasources`); }
}
