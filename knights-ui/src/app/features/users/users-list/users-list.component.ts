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
import { UserService } from '../../../core/services/user.service';
import { UserResponse } from '../../../core/models/user.model';

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
  private userService = inject(UserService);
  private fb = inject(FormBuilder);
  private confirmSvc = inject(ConfirmationService);
  private msgSvc = inject(MessageService);

  users = signal<UserResponse[]>([]);
  loading = signal(true);
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
    isEmailConfirmed: [false]
  });

  ngOnInit(): void {
    this.userService.getAll().subscribe({
      next: users => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.msgSvc.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users.' });
      }
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ isEmailConfirmed: false });
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

    const obs = id
      ? this.userService.update(id, {
          firstName: val.firstName!,
          midName: val.midName ?? undefined,
          lastName: val.lastName!,
          userName: val.userName!,
          email: val.email!,
          isEmailConfirmed: val.isEmailConfirmed ?? false
        })
      : this.userService.create({
          firstName: val.firstName!,
          midName: val.midName ?? undefined,
          lastName: val.lastName!,
          userName: val.userName!,
          email: val.email!,
          password: val.password ?? undefined,
          isEmailConfirmed: val.isEmailConfirmed ?? false
        });

    obs.subscribe({
      next: (saved) => {
        if (id) {
          this.users.update(list => list.map(u => u.id === id ? saved : u));
        } else {
          this.users.update(list => [saved, ...list]);
        }
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
        this.users.update(list => list.filter(u => u.id !== user.id));
        this.msgSvc.add({ severity: 'success', summary: 'Deleted', detail: 'User removed.' });
      }
    });
  }
}
