import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Loading } from '../../loading/loading';
import { Stars } from '../../stars/stars';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-dashboard-overview',
  imports: [RouterLink, Loading, Stars],
  templateUrl: './dashboard-overview.html',
  styleUrl: './dashboard-overview.scss',
})
export class DashboardOverview implements OnInit {
  private readonly newsService = inject(NewsService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly total = signal(0);
  protected readonly published = signal(0);
  protected readonly drafts = signal(0);
  protected readonly reads = signal(0);
  protected readonly stars = signal(0);

  ngOnInit(): void {
    this.newsService.getMine().subscribe({
      next: (stories) => {
        this.total.set(stories.length);
        this.published.set(stories.filter((story) => story.isPublished).length);
        this.drafts.set(stories.filter((story) => !story.isPublished).length);
        this.reads.set(stories.reduce((sum, story) => sum + story.viewCount, 0));
        this.stars.set(stories[0]?.authorStars ?? 0);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your dashboard.');
        this.loading.set(false);
      },
    });
  }
}
