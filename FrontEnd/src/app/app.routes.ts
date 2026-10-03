import { Routes } from '@angular/router';
import { NewsDetail } from './pages/news-detail/news-detail';
import { NewsForm } from './pages/news-form/news-form';
import { NewsList } from './pages/news-list/news-list';
import { MyNews } from './pages/my-news/my-news';
import { AuthPage } from './pages/auth-page/auth-page';
import { Dashboard } from './pages/dashboard/dashboard';
import { DashboardAccount } from './pages/dashboard/dashboard-account';
import { DashboardOverview } from './pages/dashboard/dashboard-overview';
import { Review } from './pages/review/review';
import { reporterGuard } from './reporter.guard';
import { adminGuard } from './admin.guard';

export const routes: Routes = [
  { path: '', component: NewsList },
  { path: 'news/:id', component: NewsDetail },
  { path: 'login', component: AuthPage },
  { path: 'register', component: AuthPage },
  { path: 'my-stories', redirectTo: 'dashboard/news', pathMatch: 'full' },
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [reporterGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'overview' },
      { path: 'overview', component: DashboardOverview },
      { path: 'news', component: MyNews },
      { path: 'account', component: DashboardAccount },
    ],
  },
  { path: 'admin/new', component: NewsForm, canActivate: [reporterGuard] },
  { path: 'admin/:id/edit', component: NewsForm, canActivate: [reporterGuard] },
  { path: 'review', component: Review, canActivate: [adminGuard] },
];
