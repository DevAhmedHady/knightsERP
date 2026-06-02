import { Component, output, inject } from '@angular/core';
import { PopoverModule } from 'primeng/popover';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../theme.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [PopoverModule],
  templateUrl: './topbar.component.html'
})
export class TopbarComponent {
  toggleSidebar = output<void>();
  themeService = inject(ThemeService);
  authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }

  get isSystemAdmin(): boolean {
    return this.authService.isSystemAdmin;
  }

  get tenantCodeName(): string {
    return this.authService.tenantCodeName ?? 'SYSTEM';
  }

  get displayTitle(): string {
    return this.isSystemAdmin ? 'System Command' : 'Tenant Command';
  }

  get displaySubtitle(): string {
    return this.isSystemAdmin
      ? 'Global platform controls and tenant governance'
      : 'Environment setup, members, and world operations';
  }

  get displayName(): string {
    const user = this.authService.currentUser;
    return user ? `${user.firstName} ${user.lastName}` : 'Admin User';
  }

  get displayRole(): string {
    return this.isSystemAdmin ? 'System Admin' : `Tenant · ${this.tenantCodeName}`;
  }

  get initials(): string {
    const user = this.authService.currentUser;
    if (!user) return 'AU';
    return `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase();
  }
}
