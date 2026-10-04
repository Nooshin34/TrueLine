import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DialogService } from '../../dialog/dialog.service';
import { NewsService } from '../../services/news.service';
import { AuthService } from '../../services/auth.service';
import { Loading } from '../../loading/loading';
import { newsCategories, NewsCategory } from '../../models/news';

@Component({
  selector: 'app-news-form',
  imports: [ReactiveFormsModule, Loading],
  templateUrl: './news-form.html',
  styleUrl: './news-form.scss',
})
export class NewsForm implements OnInit, OnDestroy {
  private readonly newsService = inject(NewsService);
  private readonly dialog = inject(DialogService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private articleId: number | null = null;
  private publishedAt = new Date().toISOString();
  private photoKey = 0;

  protected readonly editing = signal(false);
  protected readonly loading = signal(false);
  protected readonly unavailable = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly imageError = signal<string | null>(null);
  protected readonly photos = signal<StoryPhoto[]>([]);
  protected readonly categories = newsCategories;

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    summary: ['', [Validators.maxLength(500)]],
    body: ['', [Validators.required]],
    author: [{ value: '', disabled: true }, [Validators.required, Validators.maxLength(100)]],
    category: ['', [Validators.required]],
    isPublished: [true],
  });

  ngOnInit(): void {
    this.form.controls.author.setValue(this.auth.session()?.name ?? '');

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
          author: this.auth.session()?.name ?? article.author,
          category: article.category,
          isPublished: article.isPublished,
        });
        this.photos.set(
          (article.images ?? []).map((image) => ({
            key: `saved-${image.id}`,
            id: image.id,
            url: image.url,
            file: null,
          })),
        );
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
    if (this.photos().length < 1) {
      this.imageError.set('Add at least one photo.');
    } else if (this.photos().length > 5) {
      this.imageError.set('A story can have at most 5 photos.');
    }

    if (this.form.invalid || this.photos().length < 1 || this.photos().length > 5) {
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
    const files = this.photos().flatMap((photo) => (photo.file ? [photo.file] : []));
    const keepIds = this.photos().flatMap((photo) => (photo.id === null ? [] : [photo.id]));
    const save = this.articleId
      ? this.newsService.update(this.articleId, draft, files, keepIds)
      : this.newsService.create(draft, files);

    this.saving.set(true);
    this.error.set(null);
    save.subscribe({
      next: (saved) => {
        this.saving.set(false);
        const message = !value.isPublished
          ? 'The story is saved as a draft.'
          : this.editing()
            ? 'Your changes are saved.'
            : 'The story is saved. It appears on the site after an admin approves it.';
        this.dialog
          .success(this.editing() ? 'Story updated' : 'Story saved', message)
          .then(() => this.router.navigate(['/news', saved.id]));
      },
      error: (err: HttpErrorResponse) => {
        const body = err.error;
        const detail = typeof body === 'string' ? body : body?.detail;
        this.saving.set(false);
        this.dialog.error('Could not save the story', typeof detail === 'string' ? detail : 'Try again in a moment.');
      },
    });
  }

  protected onImagesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (files.length === 0) {
      return;
    }

    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    const next = [...this.photos()];
    let error: string | null = null;
    for (const file of files) {
      if (next.length >= 5) {
        error = 'A story can have at most 5 photos.';
        break;
      }

      if (!allowed.includes(file.type)) {
        error = 'Use a JPG, PNG, WEBP, or GIF image.';
        continue;
      }

      if (file.size > 5 * 1024 * 1024) {
        error = 'Each photo must be 5 MB or smaller.';
        continue;
      }

      this.photoKey += 1;
      next.push({
        key: String(this.photoKey),
        id: null,
        url: URL.createObjectURL(file),
        file,
      });
    }

    this.photos.set(next);
    this.imageError.set(error);
  }

  protected removePhoto(key: string): void {
    const photo = this.photos().find((item) => item.key === key);
    if (photo?.url.startsWith('blob:')) {
      URL.revokeObjectURL(photo.url);
    }

    const next = this.photos().filter((item) => item.key !== key);
    this.photos.set(next);
    if (next.length <= 5 && this.imageError() === 'A story can have at most 5 photos.') {
      this.imageError.set(null);
    }
    if (next.length > 0 && this.imageError() === 'Add at least one photo.') {
      this.imageError.set(null);
    }
  }

  ngOnDestroy(): void {
    for (const photo of this.photos()) {
      if (photo.url.startsWith('blob:')) {
        URL.revokeObjectURL(photo.url);
      }
    }
  }
}

interface StoryPhoto {
  key: string;
  id: number | null;
  url: string;
  file: File | null;
}
