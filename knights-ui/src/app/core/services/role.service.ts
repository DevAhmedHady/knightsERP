import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RoleResponse, RoleWithPermissionsResponse, CreateRoleRequest, UpdateRoleRequest } from '../models/role.model';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getAll(): Observable<RoleResponse[]> {
    return this.http.get<RoleResponse[]>(`${this.base}/api/roles`);
  }

  getById(id: string): Observable<RoleResponse> {
    return this.http.get<RoleResponse>(`${this.base}/api/roles/${id}`);
  }

  create(req: CreateRoleRequest): Observable<RoleResponse> {
    return this.http.post<RoleResponse>(`${this.base}/api/roles`, req);
  }

  update(id: string, req: UpdateRoleRequest): Observable<RoleResponse> {
    return this.http.put<RoleResponse>(`${this.base}/api/roles/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/roles/${id}`);
  }

  assignPermission(roleId: string, permissionId: string): Observable<RoleWithPermissionsResponse> {
    return this.http.post<RoleWithPermissionsResponse>(`${this.base}/api/roles/${roleId}/permissions/${permissionId}`, {});
  }

  removePermission(roleId: string, permissionId: string): Observable<RoleWithPermissionsResponse> {
    return this.http.delete<RoleWithPermissionsResponse>(`${this.base}/api/roles/${roleId}/permissions/${permissionId}`);
  }
}
