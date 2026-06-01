import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TenantResponse, CreateTenantRequest, UpdateTenantRequest } from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/api/tenants`;

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
}
