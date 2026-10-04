import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, distinctUntilChanged, map, of, switchMap, tap } from 'rxjs';
import { News, NewsCategory, newsCategories } from '../../models/news';
import { coverStyle, initial, readMinutes, timeAgo } from '../../news-format';
import { Loading } from '../../loading/loading';
import { Stars } from '../../stars/stars';
import { NewsService } from '../../services/news.service';

@Component({
  selector: 'app-news-list',
  imports: [RouterLink, Loading, Stars],
  templateUrl: './news-list.html',
  styleUrl: './news-list.scss',
})
export class NewsList implements OnInit {
  private readonly newsService = inject(NewsService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly news = signal<News[]>([]);
  private readonly categoryParam = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('category'))),
    { initialValue: this.route.snapshot.queryParamMap.get('category') },
  );
  protected readonly category = computed(() => {
    const value = this.categoryParam();
    return newsCategories.includes(value as NewsCategory) ? (value as NewsCategory) : null;
  });
  protected readonly query = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('q')?.trim() ?? '')),
    { initialValue: this.route.snapshot.queryParamMap.get('q')?.trim() ?? '' },
  );
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly stories = computed(() => {
    const category = this.category();
    const items = this.news();
    return category ? items.filter((item) => item.category === category) : items;
  });
  protected readonly featured = computed(() => this.stories()[0] ?? null);
  protected readonly picks = computed(() => this.stories().slice(1, 4));
  protected readonly latest = computed(() => {
    const items = this.stories();
    return items.length > 4 ? items.slice(4) : items.slice(1);
  });

  protected readonly coverStyle = coverStyle;
  protected readonly initial = initial;
  protected readonly timeAgo = timeAgo;
  protected readonly readMinutes = readMinutes;

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(
        map((params) => params.get('q')?.trim() ?? ''),
        distinctUntilChanged(),
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap((term) =>
          this.newsService.getPublished(term).pipe(
            catchError(() => {
              this.error.set('Could not load news.');
              return of([] as News[]);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((items) => {
        this.news.set(items);
        this.loading.set(false);
      });
  }
}
