import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RoleService } from '../../../core/services/role.service';
import { PermissionService } from '../../../core/services/permission.service';
import { RoleResponse, RoleWithPermissionsResponse } from '../../../core/models/role.model';
import { PermissionResponse } from '../../../core/models/permission.model';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [
    TableModule, ButtonModule, DialogModule, ConfirmDialogModule,
    ToastModule, InputTextModule, CheckboxModule, ReactiveFormsModule, FormsModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './roles-list.component.html'
})
export class RolesListComponent implements OnInit {
  private roleService = inject(RoleService);
  private permissionService = inject(PermissionService);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  roles = signal<RoleResponse[]>([]);
  allPermissions = signal<PermissionResponse[]>([]);
  loading = signal(true);
  dialogVisible = signal(false);
  permDialogVisible = signal(false);
  editingId = signal<string | null>(null);
  selectedRole = signal<RoleWithPermissionsResponse | null>(null);
  selectedPermIds = signal<Set<string>>(new Set());

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    isStatic: [false],
    isDefault: [false],
    isActive: [true]
  });

  ngOnInit(): void {
    this.roleService.getAll().subscribe({
      next: roles => { this.roles.set(roles); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.permissionService.getAll().subscribe(perms => this.allPermissions.set(perms));
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ isActive: true, isStatic: false, isDefault: false });
    this.dialogVisible.set(true);
  }

  openEdit(role: RoleResponse): void {
    this.editingId.set(role.id);
    this.form.patchValue(role);
    this.dialogVisible.set(true);
  }

  save(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    const id = this.editingId();

    const obs = id
      ? this.roleService.update(id, { name: val.name!, description: val.description ?? undefined })
      : this.roleService.create({
          name: val.name!,
          description: val.description ?? undefined,
          isStatic: val.isStatic ?? false,
          isDefault: val.isDefault ?? false,
          isActive: val.isActive ?? true
        });

    obs.subscribe({
      next: (saved) => {
        if (id) {
          this.roles.update(list => list.map(r => r.id === id ? saved : r));
        } else {
          this.roles.update(list => [saved, ...list]);
        }
        this.msgSvc.add({ severity: 'success', summary: id ? 'Updated' : 'Created', detail: 'Role saved.' });
        this.dialogVisible.set(false);
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
    });
  }

  confirmDelete(role: RoleResponse): void {
    this.confirmSvc.confirm({
      message: `Delete role "${role.name}"?`,
      accept: () => {
        this.roleService.delete(role.id).subscribe({
          next: () => {
            this.roles.update(list => list.filter(r => r.id !== role.id));
            this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'Role removed.' });
          },
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.' })
        });
      }
    });
  }

  openPermissions(role: RoleResponse): void {
    this.roleService.getById(role.id).subscribe(r => {
      const withPerms = r as unknown as RoleWithPermissionsResponse;
      this.selectedRole.set(withPerms);
      this.selectedPermIds.set(new Set((withPerms.permissions ?? []).map(p => p.id)));
      this.permDialogVisible.set(true);
    });
  }

  togglePermission(permId: string, roleId: string): void {
    const current = this.selectedPermIds();
    if (current.has(permId)) {
      this.roleService.removePermission(roleId, permId).subscribe({
        next: () => this.selectedPermIds.update(s => { const ns = new Set(s); ns.delete(permId); return ns; }),
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove permission.' })
      });
    } else {
      this.roleService.assignPermission(roleId, permId).subscribe({
        next: () => this.selectedPermIds.update(s => new Set([...s, permId])),
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign permission.' })
      });
    }
  }
}
