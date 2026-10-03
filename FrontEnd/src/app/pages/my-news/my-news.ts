import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DialogService } from '../../dialog/dialog.service';
import { News } from '../../models/news';
import { Loading } from '../../loading/loading';
import { timeAgo } from '../../news-format';
import { Stars } from '../../stars/stars';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-my-news',
  imports: [RouterLink, Loading, Stars],
  templateUrl: './my-news.html',
  styleUrl: './my-news.scss',
})
export class MyNews implements OnInit {
  private readonly newsService = inject(NewsService);
  private readonly dialog = inject(DialogService);

  protected readonly stories = signal<News[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly deletingId = signal<number | null>(null);
  protected readonly timeAgo = timeAgo;
  protected readonly totalReads = computed(() =>
    this.stories().reduce((sum, story) => sum + story.viewCount, 0),
  );
  protected readonly stars = computed(() => this.stories()[0]?.authorStars ?? 0);

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
    this.dialog.confirm('Delete this story?', `"${story.title}" will be removed.`).then((accepted) => {
      if (!accepted) {
        return;
      }

      this.deletingId.set(story.id);
      this.newsService.delete(story.id).subscribe({
        next: () => {
          this.stories.update((items) => items.filter((item) => item.id !== story.id));
          this.deletingId.set(null);
        },
        error: (err: HttpErrorResponse) => {
          this.deletingId.set(null);
          this.dialog.error(
            'Could not delete the story',
            err.status === 404 ? 'This story is no longer available.' : 'Try again in a moment.',
          );
        },
      });
    });
  }
}
