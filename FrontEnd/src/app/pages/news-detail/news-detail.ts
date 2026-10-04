import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DialogService } from '../../dialog/dialog.service';
import { News } from '../../models/news';
import { coverStyle, initial, readMinutes, timeAgo } from '../../news-format';
import { Loading } from '../../loading/loading';
import { Stars } from '../../stars/stars';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-news-detail',
  imports: [RouterLink, Loading, Stars],
  templateUrl: './news-detail.html',
  styleUrl: './news-detail.scss',
})
export class NewsDetail implements OnInit {
  private readonly newsService = inject(NewsService);
  private readonly dialog = inject(DialogService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly article = signal<News | null>(null);
  protected readonly loading = signal(true);
  protected readonly deleting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly slide = signal(0);
  protected readonly coverStyle = coverStyle;
  protected readonly initial = initial;
  protected readonly timeAgo = timeAgo;
  protected readonly readMinutes = readMinutes;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.error.set('Article not found.');
      this.loading.set(false);
      return;
    }

    this.newsService.getById(id).subscribe({
      next: (article) => {
        this.article.set({ ...article, images: article.images ?? [] });
        this.slide.set(0);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Article not found.');
        this.loading.set(false);
      },
    });
  }

  protected showPhoto(index: number): void {
    this.slide.set(index);
  }

  protected previousPhoto(count: number): void {
    this.slide.update((current) => (current - 1 + count) % count);
  }

  protected nextPhoto(count: number): void {
    this.slide.update((current) => (current + 1) % count);
  }

  protected remove(): void {
    const article = this.article();
    if (!article) {
      return;
    }

    this.dialog.confirm('Delete this story?', `"${article.title}" will be removed.`).then((accepted) => {
      if (!accepted) {
        return;
      }

      this.deleting.set(true);
      this.newsService.delete(article.id).subscribe({
        next: () => {
          this.router.navigate(['/dashboard/news']);
        },
        error: () => {
          this.deleting.set(false);
          this.dialog.error('Could not delete the story', 'Try again in a moment.');
        },
      });
    });
  }
}
