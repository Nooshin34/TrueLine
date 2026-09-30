import { Routes } from '@angular/router';
import { NewsDetail } from './pages/news-detail/news-detail';
import { NewsForm } from './pages/news-form/news-form';
import { NewsList } from './pages/news-list/news-list';
import { AuthPage } from './pages/auth-page/auth-page';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', component: NewsList },
  { path: 'news/:id', component: NewsDetail },
  { path: 'login', component: AuthPage },
  { path: 'register', component: AuthPage },
  { path: 'admin/new', component: NewsForm, canActivate: [authGuard] },
];
