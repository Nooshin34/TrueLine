import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { News } from '../models/news';

type NewsDraft = Omit<News, 'id' | 'imageUrl' | 'userId' | 'canEdit'>;

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

  getMine(): Observable<News[]> {
    return this.http.get<News[]>(`${this.apiUrl}/mine`);
  }

  create(news: NewsDraft, image?: File | null): Observable<News> {
    return this.http.post<News>(this.apiUrl, this.toFormData(news, image));
  }

  update(id: number, news: NewsDraft, image?: File | null): Observable<News> {
    return this.http.put<News>(`${this.apiUrl}/${id}`, this.toFormData(news, image));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private toFormData(news: NewsDraft, image?: File | null): FormData {
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

    return data;
  }
}
