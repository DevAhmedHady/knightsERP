import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
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
  collapsed = input(false);
  closeSidebar = output<void>();

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'pi pi-home', route: '/dashboard' },
    { label: 'Users', icon: 'pi pi-users', route: '/users' },
    { label: 'Roles', icon: 'pi pi-shield', route: '/roles' },
    { label: 'Permissions', icon: 'pi pi-lock', route: '/permissions' }
  ];
}
