import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TenantService } from '../../../core/services/tenant.service';
import { FeatureCatalogItemResponse, TenantSetupSummaryResponse } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, ToastModule],
  providers: [MessageService],
  templateUrl: './tenant-setup.component.html'
})
export class TenantSetupComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly fb = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly summary = signal<TenantSetupSummaryResponse | null>(null);
  readonly selectedFeatureIds = computed(() => new Set(this.summary()?.selectedFeatures.map(feature => feature.id) ?? []));

  readonly form = this.fb.group({
    environmentDisplayName: ['', Validators.required],
    themeKey: ['', Validators.required],
    worldDescription: ['', Validators.required]
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.tenantService.getCurrentSetup().subscribe({
      next: summary => {
        this.summary.set(summary);
        this.form.patchValue({
          environmentDisplayName: summary.environmentDisplayName,
          themeKey: summary.themeKey,
          worldDescription: summary.worldDescription
        });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load setup.' });
      }
    });
  }

  saveEnvironment(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();
    this.tenantService.configureCurrentEnvironment({
      environmentDisplayName: raw.environmentDisplayName ?? '',
      themeKey: raw.themeKey ?? '',
      worldDescription: raw.worldDescription ?? ''
    }).subscribe({
      next: summary => {
        this.summary.set(summary);
        this.saving.set(false);
        this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Environment updated.' });
      },
      error: () => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save environment.' });
      }
    });
  }

  toggleFeature(feature: FeatureCatalogItemResponse): void {
    const selected = this.selectedFeatureIds().has(feature.id);
    const request = selected
      ? this.tenantService.removeCurrentFeature(feature.id)
      : this.tenantService.selectCurrentFeature(feature.id);

    request.subscribe({
      next: summary => {
        this.summary.set(summary);
        this.messageService.add({
          severity: 'success',
          summary: selected ? 'Removed' : 'Added',
          detail: `${feature.name} ${selected ? 'removed from' : 'added to'} environment.`
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error?.error?.error ?? 'Feature update failed.'
        });
      }
    });
  }
}
