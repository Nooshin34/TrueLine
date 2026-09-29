import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

const storageKey = 'trueline-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>(this.readInitial());

  constructor() {
    this.apply(this.theme());
  }

  toggle(): void {
    this.set(this.theme() === 'dark' ? 'light' : 'dark');
  }

  set(theme: Theme): void {
    this.theme.set(theme);
    localStorage.setItem(storageKey, theme);
    this.apply(theme);
  }

  private readInitial(): Theme {
    const saved = localStorage.getItem(storageKey);
    if (saved === 'light' || saved === 'dark') {
      return saved;
    }

    const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)');
    return prefersDark?.matches ? 'dark' : 'light';
  }

  private apply(theme: Theme): void {
    document.documentElement.dataset['theme'] = theme;
  }
}
