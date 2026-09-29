import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { News } from '../../models/news';
import { coverStyle, initial, readMinutes, timeAgo } from '../../news-format';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-news-list',
  imports: [RouterLink],
  templateUrl: './news-list.html',
  styleUrl: './news-list.scss',
})
export class NewsList implements OnInit {
  private readonly newsService = inject(NewsService);

  protected readonly news = signal<News[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly featured = computed(() => this.news()[0] ?? null);
  protected readonly picks = computed(() => this.news().slice(1, 4));
  protected readonly latest = computed(() => {
    const items = this.news();
    return items.length > 4 ? items.slice(4) : items.slice(1);
  });

  protected readonly coverStyle = coverStyle;
  protected readonly initial = initial;
  protected readonly timeAgo = timeAgo;
  protected readonly readMinutes = readMinutes;

  ngOnInit(): void {
    this.newsService.getPublished().subscribe({
      next: (items) => {
        this.news.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load news.');
        this.loading.set(false);
      },
    });
  }
}
