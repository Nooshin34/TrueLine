import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
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
    localStorage.removeItem(storageKey);
    this.session.set(null);
  }

  private store(response: AuthResponse): void {
    const session: AuthSession = {
      token: response.token,
      name: response.name,
      email: response.email,
    };
    localStorage.setItem(storageKey, JSON.stringify(session));
    this.session.set(session);
  }

  private read(): AuthSession | null {
    const raw = localStorage.getItem(storageKey);
    if (!raw) {
      return null;
    }

    try {
      const session = JSON.parse(raw) as AuthSession;
      return session.token ? session : null;
    } catch {
      return null;
    }
  }
}
