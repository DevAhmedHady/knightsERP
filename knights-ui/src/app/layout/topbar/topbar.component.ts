import { Component, output, inject } from '@angular/core';
import { ThemeService } from '../theme.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [],
  templateUrl: './topbar.component.html'
})
export class TopbarComponent {
  toggleSidebar = output<void>();
  themeService = inject(ThemeService);
}
