import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { select, Store } from '@ngxs/store';
import { TenantResponse } from '../../../core/models/tenant.model';
import { UserResponse } from '../../../core/models/user.model';
import { LoadRoles } from '../../roles/state/roles.actions';
import { RolesState } from '../../roles/state/roles.state';
import { LoadUsers } from '../../users/state/users.actions';
import { UsersState } from '../../users/state/users.state';
import {
  AddTenantMember,
  AssignTenantRole,
  CreateTenant,
  DeactivateTenant,
  LoadTenant,
  LoadTenants,
  RemoveTenantRole,
  UpdateTenant
} from '../state/tenants.actions';
import { TenantsState } from '../state/tenants.state';

interface UserSelectOption {
  label: string;
  value: string;
  email: string;
}

@Component({
  selector: 'app-tenants-list',
  standalone: true,
  imports: [
    DatePipe,
    TableModule, ButtonModule, DialogModule, ConfirmDialogModule,
    ToastModule, InputTextModule, SelectModule, SkeletonModule, TooltipModule,
    ReactiveFormsModule, FormsModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './tenants-list.component.html'
})
export class TenantsListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  tenants = select(TenantsState.items);
  loading = select(TenantsState.loading);
  dialogVisible = signal(false);
  editingId = signal<string | null>(null);
  selectedTenant = select(TenantsState.selected);
  manageDialogVisible = signal(false);
  roleIdInput = signal('');
  users = select(UsersState.items);
  allRoles = select(RolesState.items);
  roleOptions = computed(() => this.allRoles().map(r => ({ label: r.name, value: r.id })));
  availableRoleOptions = computed(() => {
    const assigned = new Set(this.selectedTenant()?.roleIds ?? []);
    return this.roleOptions().filter(o => !assigned.has(o.value));
  });
  activeTenants = computed(() => this.tenants().filter(tenant => tenant.isActive).length);
  unlockedTenants = computed(() => this.tenants().filter(tenant => !!tenant.setupCompletedAt).length);
  ownerUserOptions = computed(() => this.users().map(user => this.toUserOption(user)));
  availableMemberUserOptions = computed(() => {
    const selected = this.selectedTenant();
    return this.users()
      .filter(user => !selected || user.tenantId !== selected.id)
      .map(user => this.toUserOption(user));
  });

  form = this.fb.group({
    codeName: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    ownerId: ['', Validators.required],
    expiryDate: [''],
    sessionTimeoutMinutes: [60, Validators.required]
  });

  memberForm = this.fb.group({
    userId: ['', Validators.required]
  });

  ngOnInit(): void {
    this.store.dispatch([new LoadTenants(), new LoadUsers(), new LoadRoles()]).subscribe({
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load tenant data.' })
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ sessionTimeoutMinutes: 60 });
    this.form.get('codeName')?.enable();
    this.dialogVisible.set(true);
  }

  openEdit(tenant: TenantResponse): void {
    this.editingId.set(tenant.id);
    this.form.patchValue({
      codeName: tenant.codeName,
      name: tenant.name,
      description: tenant.description,
      ownerId: tenant.ownerId,
      expiryDate: tenant.expiryDate ?? '',
      sessionTimeoutMinutes: tenant.sessionTimeoutMinutes
    });
    this.form.get('codeName')?.disable();
    this.dialogVisible.set(true);
  }

  openManage(tenant: TenantResponse): void {
    this.store.dispatch(new LoadTenant(tenant.id)).subscribe({
      next: () => {
        this.roleIdInput.set('');
        this.memberForm.reset();
        this.manageDialogVisible.set(true);
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load tenant.' })
    });
  }

  save(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    const id = this.editingId();

    if (id) {
      this.store.dispatch(new UpdateTenant(id, {
        name: val.name!,
        description: val.description ?? '',
        expiryDate: val.expiryDate || undefined,
        sessionTimeoutMinutes: val.sessionTimeoutMinutes ?? undefined
      })).subscribe({
        next: () => {
          this.msgSvc.add({ severity: 'success', summary: 'Updated', detail: 'Tenant saved.' });
          this.dialogVisible.set(false);
        },
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
      });
    } else {
      this.store.dispatch(new CreateTenant({
        codeName: val.codeName!,
        name: val.name!,
        description: val.description ?? '',
        ownerId: val.ownerId!,
        expiryDate: val.expiryDate || undefined,
        sessionTimeoutMinutes: val.sessionTimeoutMinutes ?? undefined
      })).subscribe({
        next: () => {
          this.msgSvc.add({ severity: 'success', summary: 'Created', detail: 'Tenant created.' });
          this.dialogVisible.set(false);
        },
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
      });
    }
  }

  assignRole(): void {
    const t = this.selectedTenant();
    const roleId = this.roleIdInput().trim();
    if (!t || !roleId) return;
    this.store.dispatch(new AssignTenantRole(t.id, roleId)).subscribe({
      next: () => {
        this.roleIdInput.set('');
        this.msgSvc.add({ severity: 'success', summary: 'Role assigned' });
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign role.' })
    });
  }

  removeRole(roleId: string): void {
    const t = this.selectedTenant();
    if (!t) return;
    this.store.dispatch(new RemoveTenantRole(t.id, roleId)).subscribe({
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove role.' })
    });
  }

  addMember(): void {
    const t = this.selectedTenant();
    const userId = this.memberForm.controls.userId.value?.trim();
    if (!t || !userId) return;
    this.store.dispatch(new AddTenantMember(t.id, userId)).subscribe({
      next: () => {
        this.store.dispatch(new LoadUsers());
        this.memberForm.reset();
        this.msgSvc.add({ severity: 'success', summary: 'Member added' });
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to add member.' })
    });
  }

  confirmDeactivate(tenant: TenantResponse): void {
    this.confirmSvc.confirm({
      message: `Deactivate tenant "${tenant.name}"?`,
      accept: () => {
        this.store.dispatch(new DeactivateTenant(tenant.id)).subscribe({
          next: () => this.msgSvc.add({ severity: 'success', summary: 'Deactivated', detail: 'Tenant deactivated.' }),
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to deactivate tenant.' })
        });
      }
    });
  }

  getRoleName(roleId: string): string {
    return this.allRoles().find(r => r.id === roleId)?.name ?? roleId.slice(0, 8) + '…';
  }

  userLabel(user: UserResponse): string {
    const fullName = `${user.firstName} ${user.lastName}`.trim();
    return fullName ? `${fullName} (${user.userName})` : user.userName;
  }

  private toUserOption(user: UserResponse): UserSelectOption {
    return {
      label: this.userLabel(user),
      value: user.id,
      email: user.email
    };
  }

}
