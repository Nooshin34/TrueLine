import { Routes } from '@angular/router';
import { NewsDetail } from './pages/news-detail/news-detail';
import { NewsForm } from './pages/news-form/news-form';
import { NewsList } from './pages/news-list/news-list';

export const routes: Routes = [
  { path: '', component: NewsList },
  { path: 'news/:id', component: NewsDetail },
  { path: 'admin/new', component: NewsForm },
];
