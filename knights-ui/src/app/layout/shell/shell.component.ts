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
  sidebarCollapsed = signal(typeof window !== 'undefined' ? window.innerWidth < 1024 : false);

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth < 1024) {
      this.sidebarCollapsed.set(true);
    }
  }
}
