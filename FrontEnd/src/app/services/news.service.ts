import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { News } from '../models/news';

@Injectable({ providedIn: 'root' })
export class NewsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5230/api/news';

  getPublished(): Observable<News[]> {
    return this.http.get<News[]>(this.apiUrl);
  }

  getById(id: number): Observable<News> {
    return this.http.get<News>(`${this.apiUrl}/${id}`);
  }

  create(news: Omit<News, 'id' | 'imageUrl'>, image?: File | null): Observable<News> {
    const data = new FormData();
    data.append('title', news.title);
    data.append('summary', news.summary ?? '');
    data.append('body', news.body);
    data.append('author', news.author);
    data.append('category', news.category);
    data.append('publishedAt', news.publishedAt);
    data.append('isPublished', String(news.isPublished));
    if (image) {
      data.append('image', image);
    }

    return this.http.post<News>(this.apiUrl, data);
  }

  update(id: number, news: Omit<News, 'id' | 'imageUrl'>): Observable<News> {
    return this.http.put<News>(`${this.apiUrl}/${id}`, news);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
