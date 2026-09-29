import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { News } from '../../models/news';
import { coverStyle, initial, readMinutes, timeAgo } from '../../news-format';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-news-detail',
  imports: [RouterLink],
  templateUrl: './news-detail.html',
  styleUrl: './news-detail.scss',
})
export class NewsDetail implements OnInit {
  private readonly newsService = inject(NewsService);
  private readonly route = inject(ActivatedRoute);

  protected readonly article = signal<News | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
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
        this.article.set(article);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Article not found.');
        this.loading.set(false);
      },
    });
  }
}
