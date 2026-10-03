import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Loading } from '../../loading/loading';
import { News } from '../../models/news';
import { Stars } from '../../stars/stars';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-review',
  imports: [RouterLink, Loading, Stars],
  templateUrl: './review.html',
  styleUrl: './review.scss',
})
export class Review implements OnInit {
  private readonly newsService = inject(NewsService);

  protected readonly stories = signal<News[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly busyId = signal<number | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected setApproval(story: News, approved: boolean): void {
    this.busyId.set(story.id);
    this.error.set(null);
    const request = approved
      ? this.newsService.approve(story.id)
      : this.newsService.unapprove(story.id);
    request.subscribe({
      next: () => {
        this.busyId.set(null);
        this.loadStories();
      },
      error: () => {
        this.busyId.set(null);
        this.error.set(approved ? 'Could not approve the story.' : 'Could not return the story to pending.');
      },
    });
  }

  private load(): void {
    this.loading.set(true);
    this.loadStories();
  }

  private loadStories(): void {
    this.newsService.getReview().subscribe({
      next: (stories) => {
        this.stories.set(stories);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load stories waiting for review.');
        this.loading.set(false);
      },
    });
  }

}
