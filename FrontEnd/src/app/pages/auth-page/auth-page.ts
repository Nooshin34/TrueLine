import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
})
export class AuthPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly registerForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });
  protected readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected isRegister(): boolean {
    return this.router.url.startsWith('/register');
  }

  protected submitRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const value = this.registerForm.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    this.auth.register(value.name.trim(), value.email.trim(), value.password).subscribe({
      next: () => this.afterAuth(),
      error: (err: HttpErrorResponse) => this.showError(err, 'Could not create the account.'),
    });
  }

  protected submitLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const value = this.loginForm.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    this.auth.login(value.email.trim(), value.password).subscribe({
      next: () => this.afterAuth(),
      error: (err: HttpErrorResponse) => this.showError(err, 'Could not sign in.'),
    });
  }

  private afterAuth(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    const target = returnUrl?.startsWith('/') ? returnUrl : '/';
    this.router.navigateByUrl(target);
  }

  private showError(err: HttpErrorResponse, fallback: string): void {
    const body = err.error;
    this.error.set(typeof body === 'string' ? body : fallback);
    this.saving.set(false);
  }
}
