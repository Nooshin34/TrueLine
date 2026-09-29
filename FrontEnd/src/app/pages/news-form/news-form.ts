import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnDestroy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NewsService } from '../../services/news.service';
import { newsCategories, NewsCategory } from '../../models/news';

@Component({
  selector: 'app-news-form',
  imports: [ReactiveFormsModule],
  templateUrl: './news-form.html',
  styleUrl: './news-form.scss',
})
export class NewsForm implements OnDestroy {
  private readonly newsService = inject(NewsService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly imageFile = signal<File | null>(null);
  protected readonly imageError = signal<string | null>(null);
  protected readonly previewUrl = signal<string | null>(null);
  protected readonly categories = newsCategories;

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    summary: ['', [Validators.maxLength(500)]],
    body: ['', [Validators.required]],
    author: ['', [Validators.required, Validators.maxLength(100)]],
    category: ['', [Validators.required]],
    isPublished: [true],
  });

  protected submit(): void {
    if (this.form.invalid || this.imageError()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);

    this.newsService
      .create(
        {
          title: value.title,
          summary: value.summary.trim() ? value.summary.trim() : null,
          body: value.body,
          author: value.author,
          category: value.category as NewsCategory,
          publishedAt: new Date().toISOString(),
          isPublished: value.isPublished,
        },
        this.imageFile(),
      )
      .subscribe({
        next: (created) => {
          this.router.navigate(['/news', created.id]);
        },
        error: (err: HttpErrorResponse) => {
          const detail = err.error?.detail;
          this.error.set(typeof detail === 'string' ? detail : 'Could not save the news item.');
          this.saving.set(false);
        },
      });
  }

  protected onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.clearPreview();

    if (!file) {
      this.imageFile.set(null);
      this.imageError.set(null);
      return;
    }

    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!allowed.includes(file.type)) {
      this.imageFile.set(null);
      this.imageError.set('Use a JPG, PNG, WEBP, or GIF image.');
      input.value = '';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.imageFile.set(null);
      this.imageError.set('Image must be 5 MB or smaller.');
      input.value = '';
      return;
    }

    this.imageFile.set(file);
    this.imageError.set(null);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  ngOnDestroy(): void {
    this.clearPreview();
  }

  private clearPreview(): void {
    const url = this.previewUrl();
    if (url) {
      URL.revokeObjectURL(url);
      this.previewUrl.set(null);
    }
  }
}
