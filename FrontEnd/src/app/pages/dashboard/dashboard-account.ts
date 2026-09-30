import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard-account',
  templateUrl: './dashboard-account.html',
  styleUrl: './dashboard-account.scss',
})
export class DashboardAccount {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly session = this.auth.session;

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
