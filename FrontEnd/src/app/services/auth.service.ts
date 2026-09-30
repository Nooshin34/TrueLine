import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthResponse, AuthSession } from '../models/auth';

const storageKey = 'trueline-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5230/api/auth';

  readonly session = signal<AuthSession | null>(this.read());
  readonly isLoggedIn = computed(() => this.session() !== null);

  register(name: string, email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/register`, { name, email, password })
      .pipe(tap((response) => this.store(response)));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(tap((response) => this.store(response)));
  }

  logout(): void {
    this.clearAvatarUrl();
    localStorage.removeItem(storageKey);
    this.session.set(null);
  }

  loadAvatar(): void {
    const session = this.session();
    if (!session || session.avatarUrl) {
      return;
    }

    this.http.get(`${this.apiUrl}/avatar`, { responseType: 'blob' }).pipe(
      map((blob) => URL.createObjectURL(blob)),
      catchError(() => of(null)),
    ).subscribe((url) => {
      if (!url) {
        return;
      }

      this.session.update((current) => current ? { ...current, hasAvatar: true, avatarUrl: url } : current);
      this.persist();
    });
  }

  uploadAvatar(file: File): Observable<void> {
    const data = new FormData();
    data.append('file', file);
    return this.http.post(`${this.apiUrl}/avatar`, data, { responseType: 'text' }).pipe(
      switchMap(() => this.http.get(`${this.apiUrl}/avatar`, { responseType: 'blob' })),
      tap((blob) => {
        this.clearAvatarUrl();
        this.session.update((current) => current
          ? { ...current, hasAvatar: true, avatarUrl: URL.createObjectURL(blob) }
          : current);
        this.persist();
      }),
      map(() => undefined),
    );
  }

  private store(response: AuthResponse): void {
    this.clearAvatarUrl();
    this.session.set({
      token: response.token,
      name: response.name,
      email: response.email,
      hasAvatar: response.hasAvatar,
      avatarUrl: null,
    });
    this.persist();
    if (response.hasAvatar) {
      this.loadAvatar();
    }
  }

  private persist(): void {
    const session = this.session();
    if (!session) {
      return;
    }

    localStorage.setItem(storageKey, JSON.stringify({
      token: session.token,
      name: session.name,
      email: session.email,
      hasAvatar: session.hasAvatar,
    }));
  }

  private clearAvatarUrl(): void {
    const url = this.session()?.avatarUrl;
    if (url?.startsWith('blob:')) {
      URL.revokeObjectURL(url);
    }
  }

  private read(): AuthSession | null {
    const raw = localStorage.getItem(storageKey);
    if (!raw) {
      return null;
    }

    try {
      const session = JSON.parse(raw) as Partial<AuthSession>;
      if (!session.token || !session.name || !session.email) {
        return null;
      }

      return {
        token: session.token,
        name: session.name,
        email: session.email,
        hasAvatar: session.hasAvatar ?? false,
        avatarUrl: null,
      };
    } catch {
      return null;
    }
  }
}
