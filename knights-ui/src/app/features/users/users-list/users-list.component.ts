import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { SkeletonModule } from 'primeng/skeleton';
import { ConfirmationService, MessageService } from 'primeng/api';
import { select, Store } from '@ngxs/store';
import { UserResponse } from '../../../core/models/user.model';
import { CreateUser, DeleteUser, LoadUsers, UpdateUser } from '../state/users.actions';
import { UsersState } from '../state/users.state';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [
    TableModule, ButtonModule, DialogModule, ConfirmDialogModule,
    ToastModule, InputTextModule, CheckboxModule, SkeletonModule,
    ReactiveFormsModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  users = select(UsersState.items);
  loading = select(UsersState.loading);
  dialogVisible = signal(false);
  editingId = signal<string | null>(null);
  searchValue = '';
  confirmedUsers = computed(() => this.users().filter(user => user.isEmailConfirmed).length);
  unassignedUsers = computed(() => this.users().filter(user => !user.tenantId).length);

  form = this.fb.group({
    firstName: ['', Validators.required],
    midName: [''],
    lastName: ['', Validators.required],
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    isEmailConfirmed: [false],
    sessionTimeoutMinutes: [60]
  });

  ngOnInit(): void {
    this.store.dispatch(new LoadUsers()).subscribe({
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users.' })
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ isEmailConfirmed: false, sessionTimeoutMinutes: 60 });
    this.dialogVisible.set(true);
  }

  openEdit(user: UserResponse): void {
    this.editingId.set(user.id);
    this.form.patchValue(user);
    this.dialogVisible.set(true);
  }

  save(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    const id = this.editingId();

    const action = id
      ? new UpdateUser(id, {
          firstName: val.firstName!,
          midName: val.midName ?? undefined,
          lastName: val.lastName!,
          userName: val.userName!,
          email: val.email!,
          isEmailConfirmed: val.isEmailConfirmed ?? false,
          sessionTimeoutMinutes: val.sessionTimeoutMinutes ?? undefined
        })
      : new CreateUser({
          firstName: val.firstName!,
          midName: val.midName ?? undefined,
          lastName: val.lastName!,
          userName: val.userName!,
          email: val.email!,
          password: val.password ?? undefined,
          isEmailConfirmed: val.isEmailConfirmed ?? false,
          sessionTimeoutMinutes: val.sessionTimeoutMinutes ?? undefined
        });

    this.store.dispatch(action).subscribe({
      next: () => {
        this.msgSvc.add({ severity: 'success', summary: id ? 'Updated' : 'Created', detail: 'User saved.' });
        this.dialogVisible.set(false);
      },
      error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Operation failed.' })
    });
  }

  confirmDelete(user: UserResponse): void {
    this.confirmSvc.confirm({
      message: `Delete user "${user.userName}"?`,
      accept: () => {
        this.store.dispatch(new DeleteUser(user.id)).subscribe({
          next: () => this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'User deleted.' }),
          error: () => this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete user.' })
        });
      }
    });
  }
}
