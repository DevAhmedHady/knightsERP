import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { select, Store } from '@ngxs/store';
import { FeatureCatalogItemResponse } from '../../../core/models/tenant.model';
import { AuthState } from '../../auth/state/auth.state';
import { CreateFeatureCatalogItem, LoadFeatureCatalog, UpdateFeatureCatalogItem } from '../state/tenants.actions';
import { TenantsState } from '../state/tenants.state';

@Component({
  selector: 'app-feature-catalog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, DialogModule, InputTextModule, TableModule, ToastModule],
  providers: [MessageService],
  templateUrl: './feature-catalog.component.html'
})
export class FeatureCatalogComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly fb = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  readonly items = select(TenantsState.featureCatalog);
  readonly loading = select(TenantsState.loading);
  readonly isSystemAdmin = select(AuthState.isSystemAdmin);
  readonly dialogVisible = signal(false);
  readonly editing = signal<FeatureCatalogItemResponse | null>(null);

  readonly form = this.fb.group({
    key: ['', Validators.required],
    name: ['', Validators.required],
    description: ['', Validators.required],
    category: ['', Validators.required],
    iconKey: ['pi pi-box', Validators.required],
    tags: [''],
    dependencyKeys: [''],
    settingsSchemaJson: ['{}', Validators.required],
    defaultSettingsJson: ['{}', Validators.required],
    setupWeight: [10, [Validators.required, Validators.min(0), Validators.max(100)]],
    isCore: [false],
    displayOrder: [0, Validators.required],
    isPublished: [true],
    isRetired: [false]
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.store.dispatch(new LoadFeatureCatalog()).subscribe({
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load catalog.' })
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.form.reset({
      key: '',
      name: '',
      description: '',
      category: '',
      iconKey: 'pi pi-box',
      tags: '',
      dependencyKeys: '',
      settingsSchemaJson: '{}',
      defaultSettingsJson: '{}',
      setupWeight: 10,
      isCore: false,
      displayOrder: 0,
      isPublished: true,
      isRetired: false
    });
    this.form.controls.key.enable();
    this.dialogVisible.set(true);
  }

  openEdit(item: FeatureCatalogItemResponse): void {
    this.editing.set(item);
    this.form.reset({
      key: item.key,
      name: item.name,
      description: item.description,
      category: item.category,
      iconKey: item.iconKey,
      tags: item.tags.join(', '),
      dependencyKeys: item.dependencyKeys.join(', '),
      settingsSchemaJson: item.settingsSchemaJson,
      defaultSettingsJson: item.defaultSettingsJson,
      setupWeight: item.setupWeight,
      isCore: item.isCore,
      displayOrder: item.displayOrder,
      isPublished: item.isPublished,
      isRetired: item.isRetired
    });
    this.form.controls.key.disable();
    this.dialogVisible.set(true);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const dependencyKeys = (raw.dependencyKeys ?? '')
      .split(',')
      .map(value => value.trim())
      .filter(Boolean);
    const tags = (raw.tags ?? '')
      .split(',')
      .map(value => value.trim())
      .filter(Boolean);

    const action = this.editing()
      ? new UpdateFeatureCatalogItem(this.editing()!.id, {
          name: raw.name!,
          description: raw.description!,
          category: raw.category!,
          iconKey: raw.iconKey!,
          tags,
          dependencyKeys,
          settingsSchemaJson: raw.settingsSchemaJson!,
          defaultSettingsJson: raw.defaultSettingsJson!,
          setupWeight: Number(raw.setupWeight ?? 0),
          isCore: !!raw.isCore,
          displayOrder: Number(raw.displayOrder ?? 0),
          isPublished: !!raw.isPublished,
          isRetired: !!raw.isRetired
        })
      : new CreateFeatureCatalogItem({
          key: raw.key!,
          name: raw.name!,
          description: raw.description!,
          category: raw.category!,
          iconKey: raw.iconKey!,
          tags,
          dependencyKeys,
          settingsSchemaJson: raw.settingsSchemaJson!,
          defaultSettingsJson: raw.defaultSettingsJson!,
          setupWeight: Number(raw.setupWeight ?? 0),
          isCore: !!raw.isCore,
          displayOrder: Number(raw.displayOrder ?? 0),
          isPublished: !!raw.isPublished
        });

    this.store.dispatch(action).subscribe({
      next: () => {
        this.dialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Catalog updated.' });
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: error?.error?.error ?? 'Save failed.' });
      }
    });
  }
}
