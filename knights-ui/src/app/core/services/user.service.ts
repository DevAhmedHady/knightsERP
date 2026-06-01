import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserResponse, CreateUserRequest, UpdateUserRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  getById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.base}/api/users/${id}`);
  }

  create(req: CreateUserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.base}/api/users`, req);
  }

  update(id: string, req: UpdateUserRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.base}/api/users/${id}`, req);
  }

  assignRole(userId: string, roleId: string): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.base}/api/users/${userId}/roles/${roleId}`, {});
  }

  grantPermission(userId: string, permissionId: string): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.base}/api/users/${userId}/permissions/${permissionId}`, {});
  }
}
