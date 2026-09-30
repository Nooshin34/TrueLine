import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NewsService } from '../../services/news.service';
import { newsCategories, NewsCategory } from '../../models/news';

@Component({
  selector: 'app-news-form',
  imports: [ReactiveFormsModule],
  templateUrl: './news-form.html',
  styleUrl: './news-form.scss',
})
export class NewsForm implements OnInit, OnDestroy {
  private readonly newsService = inject(NewsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private articleId: number | null = null;
  private publishedAt = new Date().toISOString();

  protected readonly editing = signal(false);
  protected readonly loading = signal(false);
  protected readonly unavailable = signal(false);
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

  ngOnInit(): void {
    const raw = this.route.snapshot.paramMap.get('id');
    if (!raw) {
      return;
    }

    const id = Number(raw);
    if (!Number.isInteger(id) || id <= 0) {
      this.editing.set(true);
      this.unavailable.set(true);
      this.error.set('Article not found.');
      return;
    }

    this.editing.set(true);
    this.loading.set(true);
    this.newsService.getById(id).subscribe({
      next: (article) => {
        if (!article.canEdit) {
          this.unavailable.set(true);
          this.error.set('You can only edit your own stories.');
          this.loading.set(false);
          return;
        }

        this.articleId = article.id;
        this.publishedAt = article.publishedAt;
        this.form.patchValue({
          title: article.title,
          summary: article.summary ?? '',
          body: article.body,
          author: article.author,
          category: article.category,
          isPublished: article.isPublished,
        });
        if (article.imageUrl) {
          this.previewUrl.set(article.imageUrl);
        }
        this.loading.set(false);
      },
      error: () => {
        this.unavailable.set(true);
        this.error.set('Article not found.');
        this.loading.set(false);
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid || this.imageError()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const draft = {
      title: value.title,
      summary: value.summary.trim() ? value.summary.trim() : null,
      body: value.body,
      author: value.author,
      category: value.category as NewsCategory,
      publishedAt: this.editing() ? this.publishedAt : new Date().toISOString(),
      isPublished: value.isPublished,
    };
    const save = this.articleId
      ? this.newsService.update(this.articleId, draft, this.imageFile())
      : this.newsService.create(draft, this.imageFile());

    this.saving.set(true);
    this.error.set(null);
    save.subscribe({
      next: (saved) => {
        this.router.navigate(['/news', saved.id]);
      },
      error: (err: HttpErrorResponse) => {
        const body = err.error;
        const detail = typeof body === 'string' ? body : body?.detail;
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
    if (url?.startsWith('blob:')) {
      URL.revokeObjectURL(url);
    }
    this.previewUrl.set(null);
  }
}
