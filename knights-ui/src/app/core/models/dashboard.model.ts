export type WidgetType = 'Chart' | 'Grid';

export interface Dashboard {
  id: string;
  name: string;
  slug: string;
  widgets: DashboardWidget[];
}

export interface DashboardWidget {
  id: string;
  title: string;
  widgetType: WidgetType;
  dataSourceKey: string;
  query: WidgetQuery;
  visualization: VisualizationConfig;
  row: number;
  column: number;
  width: number;
  height: number;
}

export interface WidgetQuery {
  fields: string[];
  filters: WidgetFilter[];
  groupBy?: string;
  aggregation?: { function: string; alias: string };
  limit: number;
}

export interface WidgetFilter { field: string; operator: string; value: string; }
export interface VisualizationConfig { chartType: string; labelField?: string; valueField?: string; }
export interface DataSourceField { key: string; name: string; dataType: string; filterable: boolean; groupable: boolean; }
export interface DataSource { key: string; name: string; requiredPermission: string; fields: DataSourceField[]; }
export interface WidgetData { columns: string[]; rows: Record<string, unknown>[]; }
export interface SaveWidget { title: string; widgetType: WidgetType; dataSourceKey: string; query: WidgetQuery; visualization: VisualizationConfig; row: number; column: number; width: number; height: number; }
