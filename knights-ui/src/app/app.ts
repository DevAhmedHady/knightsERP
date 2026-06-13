import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './layout/theme.service';
import { Store } from '@ngxs/store';
import { RestoreSession } from './features/auth/state/auth.actions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class App implements OnInit {
  private themeService = inject(ThemeService);
  private store = inject(Store);

  ngOnInit(): void {
    this.themeService.init();
    this.store.dispatch(new RestoreSession());
  }
}
