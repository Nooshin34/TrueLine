import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard-account',
  templateUrl: './dashboard-account.html',
  styleUrl: './dashboard-account.scss',
})
export class DashboardAccount {
  private readonly auth = inject(AuthService);
  protected readonly session = this.auth.session;
  protected readonly error = signal<string | null>(null);
  protected readonly uploading = signal(false);
  protected readonly photoLoading = computed(() => {
    const account = this.session();
    return this.uploading() || (!!account?.hasAvatar && !account.avatarUrl);
  });

  protected initial(name: string): string {
    return name.trim().charAt(0).toUpperCase() || '?';
  }

  protected onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) {
      return;
    }

    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!allowed.includes(file.type)) {
      this.error.set('Use a JPG, PNG, WEBP, or GIF image.');
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.error.set('Photo must be 2 MB or smaller.');
      return;
    }

    this.error.set(null);
    this.uploading.set(true);
    this.auth.uploadAvatar(file).subscribe({
      next: () => this.uploading.set(false),
      error: () => {
        this.uploading.set(false);
        this.error.set('Could not save the photo.');
      },
    });
  }
}
