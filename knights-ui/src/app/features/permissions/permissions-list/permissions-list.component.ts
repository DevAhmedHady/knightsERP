import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PermissionService } from '../../../core/services/permission.service';
import { PermissionResponse } from '../../../core/models/permission.model';

@Component({
  selector: 'app-permissions-list',
  standalone: true,
  imports: [
    TableModule, ButtonModule, DialogModule, ConfirmDialogModule,
    ToastModule, InputTextModule, TextareaModule, ReactiveFormsModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './permissions-list.component.html'
})
export class PermissionsListComponent implements OnInit {
  private readonly permissionService = inject(PermissionService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmSvc = inject(ConfirmationService);
  private readonly msgSvc = inject(MessageService);

  permissions = signal<PermissionResponse[]>([]);
  loading = signal(true);
  dialogVisible = signal(false);
  editingId = signal<string | null>(null);

  form = this.fb.group({
    codeName: ['', Validators.required],
    displayName: ['', Validators.required],
    description: ['']
  });

  ngOnInit(): void {
    this.permissionService.getAll().subscribe({
      next: perms => { this.permissions.set(perms); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset();
    this.dialogVisible.set(true);
  }

  openEdit(perm: PermissionResponse): void {
    this.editingId.set(perm.id);
    this.form.patchValue(perm);
    this.form.get('codeName')?.disable();
    this.dialogVisible.set(true);
  }

  save(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    const id = this.editingId();

    const obs = id
      ? this.permissionService.update(id, {
          displayName: val.displayName!,
          description: val.description ?? undefined
        })
      : this.permissionService.create({
          codeName: val.codeName!,
          displayName: val.displayName!,
          description: val.description ?? undefined
        });

    obs.subscribe({
      next: (saved) => {
        if (id) {
          this.permissions.update(list => list.map(p => p.id === id ? saved : p));
        } else {
          this.permissions.update(list => [saved, ...list]);
        }
        this.msgSvc.add({ severity: 'success', summary: id ? 'Updated' : 'Created', detail: 'Permission saved.' });
        this.form.get('codeName')?.enable();
        this.dialogVisible.set(false);
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
    });
  }

  confirmDelete(perm: PermissionResponse): void {
    this.confirmSvc.confirm({
      message: `Delete permission "${perm.displayName}"?`,
      accept: () => {
        this.permissionService.delete(perm.id).subscribe({
          next: () => {
            this.permissions.update(list => list.filter(p => p.id !== perm.id));
            this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'Permission removed.' });
          },
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.' })
        });
      }
    });
  }
}
