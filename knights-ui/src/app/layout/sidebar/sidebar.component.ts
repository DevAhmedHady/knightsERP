import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  shortLabel: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  isDesktop = input(true);
  desktopCollapsed = input(false);
  mobileOpen = input(false);
  toggleSidebar = output<void>();
  closeSidebar = output<void>();

  navItems: NavItem[] = [
    { label: 'Dashboard', shortLabel: 'DB', icon: 'pi pi-home', route: '/dashboard' },
    { label: 'Users', shortLabel: 'US', icon: 'pi pi-users', route: '/users' },
    { label: 'Roles', shortLabel: 'RL', icon: 'pi pi-shield', route: '/roles' },
    { label: 'Permissions', shortLabel: 'PM', icon: 'pi pi-lock', route: '/permissions' }
  ];

  closeOnMobile(): void {
    if (!this.isDesktop()) {
      this.closeSidebar.emit();
    }
  }
}
