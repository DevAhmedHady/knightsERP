import { Component, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  shortLabel: string;
  icon: string;
  route: string;
  systemAdminOnly?: boolean;
  tenantOnly?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  private authService = inject(AuthService);

  isDesktop = input(true);
  desktopCollapsed = input(false);
  mobileOpen = input(false);
  toggleSidebar = output<void>();
  closeSidebar = output<void>();

  navItems: NavItem[] = [
    { label: 'Dashboard', shortLabel: 'DB', icon: 'pi pi-home', route: '/dashboard' },
    { label: 'Tenants', shortLabel: 'TN', icon: 'pi pi-building', route: '/tenants' },
    { label: 'Setup', shortLabel: 'ST', icon: 'pi pi-sparkles', route: '/setup', tenantOnly: true },
    { label: 'Feature Catalog', shortLabel: 'FC', icon: 'pi pi-sliders-h', route: '/feature-catalog', systemAdminOnly: true },
    { label: 'Users', shortLabel: 'US', icon: 'pi pi-users', route: '/users' },
    { label: 'Roles', shortLabel: 'RL', icon: 'pi pi-shield', route: '/roles' },
    { label: 'Permissions', shortLabel: 'PM', icon: 'pi pi-lock', route: '/permissions' }
  ];

  get currentUser() { return this.authService.currentUser; }
  get tenantCodeName() { return this.authService.tenantCodeName; }
  get isSystemAdmin() { return this.authService.isSystemAdmin; }

  get displayName(): string {
    const u = this.currentUser;
    return u ? `${u.firstName} ${u.lastName}` : 'Admin User';
  }

  get displayEmail(): string {
    return this.currentUser?.email ?? 'admin@knights.local';
  }

  get initials(): string {
    const u = this.currentUser;
    if (!u) return 'AU';
    return `${u.firstName[0] ?? ''}${u.lastName[0] ?? ''}`.toUpperCase();
  }

  closeOnMobile(): void {
    if (!this.isDesktop()) {
      this.closeSidebar.emit();
    }
  }

  canShow(item: NavItem): boolean {
    if (item.systemAdminOnly) {
      return this.isSystemAdmin;
    }

    if (item.tenantOnly) {
      return !this.isSystemAdmin;
    }

    return true;
  }
}
