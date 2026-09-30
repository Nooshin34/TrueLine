import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly themes = inject(ThemeService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly theme = this.themes.theme;
  protected readonly session = this.auth.session;
  protected readonly menuOpen = signal(false);

  ngOnInit(): void {
    this.auth.loadAvatar();
  }

  protected toggleTheme(): void {
    this.themes.toggle();
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected initial(name: string): string {
    return name.trim().charAt(0).toUpperCase() || '?';
  }

  protected logout(): void {
    this.menuOpen.set(false);
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
