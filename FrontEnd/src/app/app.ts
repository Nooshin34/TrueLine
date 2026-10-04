import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { Dialog } from './dialog/dialog';
import { newsCategories } from './models/news';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Dialog],
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
  protected readonly isAdmin = computed(() => this.session()?.role === 'admin');
  protected readonly avatarLoading = computed(() => {
    const account = this.session();
    return !!account && account.hasAvatar && !account.avatarUrl;
  });
  protected readonly menuOpen = signal(false);
  protected readonly categories = newsCategories;
  protected readonly year = new Date().getFullYear();
  private readonly path = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects.split('?')[0]),
      startWith(this.router.url.split('?')[0]),
    ),
    { initialValue: this.router.url.split('?')[0] },
  );
  protected readonly showTopics = computed(() => {
    const path = this.path();
    return path === '/' || path.startsWith('/news/');
  });
  protected readonly searchQuery = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(() => new URL(this.router.url, 'http://localhost').searchParams.get('q') ?? ''),
      startWith(new URL(this.router.url, 'http://localhost').searchParams.get('q') ?? ''),
    ),
    { initialValue: new URL(this.router.url, 'http://localhost').searchParams.get('q') ?? '' },
  );

  ngOnInit(): void {
    this.auth.loadAvatar();
  }

  protected search(value: string): void {
    const q = value.trim();
    this.router.navigate(['/'], {
      queryParams: { q: q || null },
      queryParamsHandling: 'merge',
    });
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
