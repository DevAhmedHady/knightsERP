import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  TenantResponse,
  CreateTenantRequest,
  UpdateTenantRequest,
  TenantSetupSummaryResponse,
  ConfigureTenantEnvironmentRequest,
  FeatureCatalogItemResponse,
  CreateFeatureCatalogItemRequest,
  UpdateFeatureCatalogItemRequest
} from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/api/tenants`;

  getAll(): Observable<TenantResponse[]> {
    return this.http.get<TenantResponse[]>(this.base);
  }

  getById(id: string): Observable<TenantResponse> {
    return this.http.get<TenantResponse>(`${this.base}/${id}`);
  }

  getByCodeName(codeName: string): Observable<TenantResponse> {
    return this.http.get<TenantResponse>(`${this.base}/code/${codeName}`);
  }

  create(req: CreateTenantRequest): Observable<TenantResponse> {
    return this.http.post<TenantResponse>(this.base, req);
  }

  update(id: string, req: UpdateTenantRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  deactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/deactivate`, {});
  }

  assignRole(tenantId: string, roleId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${tenantId}/roles/${roleId}`, {});
  }

  removeRole(tenantId: string, roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${tenantId}/roles/${roleId}`);
  }

  grantPermission(tenantId: string, permissionId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${tenantId}/permissions/${permissionId}`, {});
  }

  revokePermission(tenantId: string, permissionId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${tenantId}/permissions/${permissionId}`);
  }

  addMember(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${tenantId}/members/${userId}`, {});
  }

  removeMember(tenantId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${tenantId}/members/${userId}`);
  }

  getCurrentSetup(): Observable<TenantSetupSummaryResponse> {
    return this.http.get<TenantSetupSummaryResponse>(`${this.base}/current/setup`);
  }

  configureCurrentEnvironment(req: ConfigureTenantEnvironmentRequest): Observable<TenantSetupSummaryResponse> {
    return this.http.put<TenantSetupSummaryResponse>(`${this.base}/current/environment`, req);
  }

  selectCurrentFeature(featureId: string): Observable<TenantSetupSummaryResponse> {
    return this.http.post<TenantSetupSummaryResponse>(`${this.base}/current/features/${featureId}`, {});
  }

  removeCurrentFeature(featureId: string): Observable<TenantSetupSummaryResponse> {
    return this.http.delete<TenantSetupSummaryResponse>(`${this.base}/current/features/${featureId}`);
  }

  updateCurrentFeatureSettings(featureId: string, settingsJson: string): Observable<TenantSetupSummaryResponse> {
    return this.http.put<TenantSetupSummaryResponse>(`${this.base}/current/features/${featureId}/settings`, { settingsJson });
  }

  getFeatureCatalog(): Observable<FeatureCatalogItemResponse[]> {
    return this.http.get<FeatureCatalogItemResponse[]>(`${this.base}/features/catalog`);
  }

  createFeatureCatalogItem(req: CreateFeatureCatalogItemRequest): Observable<FeatureCatalogItemResponse> {
    return this.http.post<FeatureCatalogItemResponse>(`${this.base}/features/catalog`, req);
  }

  updateFeatureCatalogItem(featureId: string, req: UpdateFeatureCatalogItemRequest): Observable<FeatureCatalogItemResponse> {
    return this.http.put<FeatureCatalogItemResponse>(`${this.base}/features/catalog/${featureId}`, req);
  }
}
