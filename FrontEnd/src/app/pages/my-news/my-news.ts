import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { News } from '../../models/news';
import { timeAgo } from '../../news-format';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-my-news',
  imports: [RouterLink],
  templateUrl: './my-news.html',
  styleUrl: './my-news.scss',
})
export class MyNews implements OnInit {
  private readonly newsService = inject(NewsService);

  protected readonly stories = signal<News[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly deletingId = signal<number | null>(null);
  protected readonly timeAgo = timeAgo;

  ngOnInit(): void {
    this.newsService.getMine().subscribe({
      next: (stories) => {
        this.stories.set(stories);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your news.');
        this.loading.set(false);
      },
    });
  }

  protected remove(story: News): void {
    if (!confirm(`Delete "${story.title}"?`)) {
      return;
    }

    this.deletingId.set(story.id);
    this.error.set(null);
    this.newsService.delete(story.id).subscribe({
      next: () => {
        this.stories.update((items) => items.filter((item) => item.id !== story.id));
        this.deletingId.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err.status === 404 ? 'This story is no longer available.' : 'Could not delete the story.');
        this.deletingId.set(null);
      },
    });
  }
}
