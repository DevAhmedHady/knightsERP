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
import { TenantService } from '../../../core/services/tenant.service';
import { TenantResponse } from '../../../core/models/tenant.model';
import { UserService } from '../../../core/services/user.service';
import { UserResponse } from '../../../core/models/user.model';
import { RoleService } from '../../../core/services/role.service';
import { RoleResponse } from '../../../core/models/role.model';

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
  private tenantService = inject(TenantService);
  private userService = inject(UserService);
  private roleService = inject(RoleService);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  tenants = signal<TenantResponse[]>([]);
  loading = signal(true);
  dialogVisible = signal(false);
  editingId = signal<string | null>(null);
  selectedTenant = signal<TenantResponse | null>(null);
  manageDialogVisible = signal(false);
  roleIdInput = signal('');
  users = signal<UserResponse[]>([]);
  allRoles = signal<RoleResponse[]>([]);
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
    expiryDate: ['']
  });

  memberForm = this.fb.group({
    userId: ['', Validators.required]
  });

  ngOnInit(): void {
    this.loadTenants();
    this.loadUsers();
    this.roleService.getAll().subscribe({ next: roles => this.allRoles.set(roles) });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset();
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
      expiryDate: tenant.expiryDate ?? ''
    });
    this.form.get('codeName')?.disable();
    this.dialogVisible.set(true);
  }

  openManage(tenant: TenantResponse): void {
    this.tenantService.getById(tenant.id).subscribe({
      next: t => {
        this.selectedTenant.set(t);
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
      this.tenantService.update(id, {
        name: val.name!,
        description: val.description ?? '',
        expiryDate: val.expiryDate || undefined
      }).subscribe({
        next: () => {
          this.tenants.update(list => list.map(t => t.id === id
            ? { ...t, name: val.name!, description: val.description ?? '' }
            : t));
          this.msgSvc.add({ severity: 'success', summary: 'Updated', detail: 'Tenant saved.' });
          this.dialogVisible.set(false);
        },
        error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
      });
    } else {
      this.tenantService.create({
        codeName: val.codeName!,
        name: val.name!,
        description: val.description ?? '',
        ownerId: val.ownerId!,
        expiryDate: val.expiryDate || undefined
      }).subscribe({
        next: result => {
          this.tenants.update(list => [result, ...list]);
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
    this.tenantService.assignRole(t.id, roleId).subscribe({
      next: () => {
        this.selectedTenant.update(prev => prev ? { ...prev, roleIds: [...prev.roleIds, roleId] } : prev);
        this.roleIdInput.set('');
        this.msgSvc.add({ severity: 'success', summary: 'Role assigned' });
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign role.' })
    });
  }

  removeRole(roleId: string): void {
    const t = this.selectedTenant();
    if (!t) return;
    this.tenantService.removeRole(t.id, roleId).subscribe({
      next: () => this.selectedTenant.update(prev => prev
        ? { ...prev, roleIds: prev.roleIds.filter(r => r !== roleId) }
        : prev),
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to remove role.' })
    });
  }

  addMember(): void {
    const t = this.selectedTenant();
    const userId = this.memberForm.controls.userId.value?.trim();
    if (!t || !userId) return;
    this.tenantService.addMember(t.id, userId).subscribe({
      next: () => {
        this.users.update(users => users.map(user => user.id === userId ? { ...user, tenantId: t.id } : user));
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
        this.tenants.update(list => list.map(t => t.id === tenant.id ? { ...t, isActive: false } : t));
        this.msgSvc.add({ severity: 'success', summary: 'Deactivated' });
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

  private loadUsers(): void {
    this.userService.getAll().subscribe({
      next: users => this.users.set(users),
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users.' })
    });
  }

  private loadTenants(): void {
    this.loading.set(true);
    this.tenantService.getAll().subscribe({
      next: tenants => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load tenants.' });
      }
    });
  }
}
