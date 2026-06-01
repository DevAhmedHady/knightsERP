import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PermissionResponse, CreatePermissionRequest, UpdatePermissionRequest } from '../models/permission.model';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getAll(): Observable<PermissionResponse[]> {
    return this.http.get<PermissionResponse[]>(`${this.base}/api/permissions`);
  }

  getById(id: string): Observable<PermissionResponse> {
    return this.http.get<PermissionResponse>(`${this.base}/api/permissions/${id}`);
  }

  create(req: CreatePermissionRequest): Observable<PermissionResponse> {
    return this.http.post<PermissionResponse>(`${this.base}/api/permissions`, req);
  }

  update(id: string, req: UpdatePermissionRequest): Observable<PermissionResponse> {
    return this.http.put<PermissionResponse>(`${this.base}/api/permissions/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/permissions/${id}`);
  }
}
