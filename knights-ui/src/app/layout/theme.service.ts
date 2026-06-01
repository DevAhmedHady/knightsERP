import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'knights-theme';
  isDark = signal(false);

  private applyTheme(isDark: boolean): void {
    this.isDark.set(isDark);
    document.documentElement.classList.toggle('dark', isDark);
    document.documentElement.style.colorScheme = isDark ? 'dark' : 'light';
  }

  toggle(): void {
    const nextIsDark = !this.isDark();
    this.applyTheme(nextIsDark);
    localStorage.setItem(this.storageKey, nextIsDark ? 'dark' : 'light');
  }

  init(): void {
    const storedTheme = localStorage.getItem(this.storageKey);
    this.applyTheme(storedTheme === 'dark');
  }
}
