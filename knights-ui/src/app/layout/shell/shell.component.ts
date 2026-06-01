import { Component, HostListener, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  private readonly desktopBreakpoint = 1024;

  isDesktop = signal(typeof window !== 'undefined' ? window.innerWidth >= this.desktopBreakpoint : true);
  desktopSidebarCollapsed = signal(false);
  mobileSidebarOpen = signal(false);

  toggleSidebar(): void {
    if (this.isDesktop()) {
      this.desktopSidebarCollapsed.update(value => !value);
      return;
    }

    this.mobileSidebarOpen.update(value => !value);
  }

  closeSidebar(): void {
    if (!this.isDesktop()) {
      this.mobileSidebarOpen.set(false);
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    const nextIsDesktop = window.innerWidth >= this.desktopBreakpoint;
    this.isDesktop.set(nextIsDesktop);

    if (nextIsDesktop) {
      this.mobileSidebarOpen.set(false);
    }
  }
}
