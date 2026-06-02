import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TenantService } from '../../../core/services/tenant.service';
import { FeatureCatalogItemResponse, TenantSelectedFeatureResponse, TenantSetupSummaryResponse } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, CheckboxModule, ToastModule],
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
  readonly featureSettingsForms = signal<Record<string, FormGroup>>({});
  readonly completedSteps = computed(() => this.summary()?.steps.filter(step => step.isCompleted).length ?? 0);
  readonly coreFeatureCount = computed(() => this.summary()?.selectedFeatures.filter(feature => feature.isCore).length ?? 0);
  readonly optionalFeatureCount = computed(() => this.summary()?.selectedFeatures.filter(feature => !feature.isCore).length ?? 0);

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
        this.featureSettingsForms.set(this.buildFeatureSettingsForms(summary.selectedFeatures));
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
        this.featureSettingsForms.set(this.buildFeatureSettingsForms(summary.selectedFeatures));
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
        this.featureSettingsForms.set(this.buildFeatureSettingsForms(summary.selectedFeatures));
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

  featureSettingsKeys(feature: TenantSelectedFeatureResponse): string[] {
    return Object.keys(this.getFeatureSchemaProperties(feature));
  }

  featureSettingsControl(featureId: string, key: string): FormControl {
    return this.featureSettingsForms()[featureId].get(key) as FormControl;
  }

  featureSettingType(feature: TenantSelectedFeatureResponse, key: string): string {
    const property = this.getFeatureSchemaProperties(feature)[key] ?? {};
    return property.type === 'integer' ? 'number' : property.type === 'boolean' ? 'boolean' : 'string';
  }

  saveFeatureSettings(feature: TenantSelectedFeatureResponse): void {
    const form = this.featureSettingsForms()[feature.id];
    if (!form) {
      return;
    }

    const payload = JSON.stringify(form.getRawValue());
    this.tenantService.updateCurrentFeatureSettings(feature.id, payload).subscribe({
      next: summary => {
        this.summary.set(summary);
        this.featureSettingsForms.set(this.buildFeatureSettingsForms(summary.selectedFeatures));
        this.messageService.add({ severity: 'success', summary: 'Saved', detail: `${feature.name} settings updated.` });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error?.error?.error ?? 'Feature settings update failed.'
        });
      }
    });
  }

  private buildFeatureSettingsForms(features: TenantSelectedFeatureResponse[]): Record<string, FormGroup> {
    return features.reduce<Record<string, FormGroup>>((acc, feature) => {
      const properties = this.getFeatureSchemaProperties(feature);
      const settings = this.parseJson(feature.settingsJson || feature.defaultSettingsJson);
      const controls = Object.entries(properties).reduce<Record<string, FormControl>>((map, [key, meta]) => {
        map[key] = new FormControl(settings[key] ?? this.defaultValueFor(meta.type));
        return map;
      }, {});
      acc[feature.id] = new FormGroup(controls);
      return acc;
    }, {});
  }

  private getFeatureSchemaProperties(feature: TenantSelectedFeatureResponse): Record<string, { type?: string }> {
    return this.parseJson(feature.settingsSchemaJson).properties ?? {};
  }

  private parseJson(value: string): any {
    try {
      return JSON.parse(value || '{}');
    } catch {
      return {};
    }
  }

  private defaultValueFor(type?: string): string | number | boolean {
    switch (type) {
      case 'integer':
        return 0;
      case 'boolean':
        return false;
      default:
        return '';
    }
  }
}
