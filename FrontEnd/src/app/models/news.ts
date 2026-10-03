export const newsCategories = [
  'World',
  'Politics',
  'Business',
  'Sport',
  'Culture',
  'Technology',
  'Science',
] as const;

export type NewsCategory = (typeof newsCategories)[number];

export interface News {
  id: number;
  title: string;
  summary: string | null;
  body: string;
  author: string;
  category: NewsCategory;
  publishedAt: string;
  isPublished: boolean;
  isApproved: boolean;
  viewCount: number;
  authorStars: number;
  reporterId: number | null;
  canEdit: boolean;
  imageUrl: string | null;
}
