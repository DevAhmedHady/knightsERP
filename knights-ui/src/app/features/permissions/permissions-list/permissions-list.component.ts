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
import { select, Store } from '@ngxs/store';
import { PermissionResponse } from '../../../core/models/permission.model';
import { CreatePermission, DeletePermission, LoadPermissions, UpdatePermission } from '../state/permissions.actions';
import { PermissionsState } from '../state/permissions.state';

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
  private readonly store = inject(Store);
  private readonly fb = inject(FormBuilder);
  private readonly confirmSvc = inject(ConfirmationService);
  private readonly msgSvc = inject(MessageService);

  permissions = select(PermissionsState.items);
  loading = select(PermissionsState.loading);
  dialogVisible = signal(false);
  editingId = signal<string | null>(null);

  form = this.fb.group({
    codeName: ['', Validators.required],
    displayName: ['', Validators.required],
    description: ['']
  });

  ngOnInit(): void {
    this.store.dispatch(new LoadPermissions());
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.get('codeName')?.enable();
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

    const action = id
      ? new UpdatePermission(id, {
          displayName: val.displayName!,
          description: val.description ?? undefined
        })
      : new CreatePermission({
          codeName: val.codeName!,
          displayName: val.displayName!,
          description: val.description ?? undefined
        });

    this.store.dispatch(action).subscribe({
      next: () => {
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
        this.store.dispatch(new DeletePermission(perm.id)).subscribe({
          next: () => {
            this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'Permission removed.' });
          },
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.' })
        });
      }
    });
  }
}
