import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmationService, MessageService } from 'primeng/api';
import { select, Store } from '@ngxs/store';
import { RoleResponse } from '../../../core/models/role.model';
import { LoadPermissions } from '../../permissions/state/permissions.actions';
import { PermissionsState } from '../../permissions/state/permissions.state';
import {
  AssignRolePermission,
  CreateRole,
  DeleteRole,
  LoadRole,
  LoadRoles,
  RemoveRolePermission,
  UpdateRole
} from '../state/roles.actions';
import { RolesState } from '../state/roles.state';

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
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  roles = select(RolesState.items);
  allPermissions = select(PermissionsState.items);
  loading = select(RolesState.loading);
  dialogVisible = signal(false);
  permDialogVisible = signal(false);
  editingId = signal<string | null>(null);
  selectedRole = select(RolesState.selected);
  selectedPermIds = computed(() => new Set(this.selectedRole()?.permissions?.map(permission => permission.id) ?? []));
  activeRoles = computed(() => this.roles().filter(r => r.isActive).length);
  staticRoles = computed(() => this.roles().filter(r => r.isStatic).length);

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    isStatic: [false],
    isDefault: [false],
    isActive: [true]
  });

  ngOnInit(): void {
    this.store.dispatch([new LoadRoles(), new LoadPermissions()]);
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

    const action = id
      ? new UpdateRole(id, { name: val.name!, description: val.description ?? undefined })
      : new CreateRole({
          name: val.name!,
          description: val.description ?? undefined,
          isStatic: val.isStatic ?? false,
          isDefault: val.isDefault ?? false,
          isActive: val.isActive ?? true
        });

    this.store.dispatch(action).subscribe({
      next: () => {
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
        this.store.dispatch(new DeleteRole(role.id)).subscribe({
          next: () => {
            this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'Role removed.' });
          },
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.' })
        });
      }
    });
  }

  openPermissions(role: RoleResponse): void {
    this.store.dispatch(new LoadRole(role.id)).subscribe({
      next: () => this.permDialogVisible.set(true),
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load role.' })
    });
  }

  togglePermission(permId: string, roleId: string): void {
    const current = this.selectedPermIds();
    if (current.has(permId)) {
      this.store.dispatch(new RemoveRolePermission(roleId, permId)).subscribe({
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove permission.' })
      });
    } else {
      this.store.dispatch(new AssignRolePermission(roleId, permId)).subscribe({
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign permission.' })
      });
    }
  }
}
