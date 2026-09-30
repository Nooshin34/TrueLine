import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  private readonly themes = inject(ThemeService);
  private readonly auth = inject(AuthService);
  protected readonly theme = this.themes.theme;
  protected readonly session = this.auth.session;

  protected toggleTheme(): void {
    this.themes.toggle();
  }

  protected logout(): void {
    this.auth.logout();
  }
}
