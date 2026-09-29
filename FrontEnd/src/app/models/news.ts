export interface News {
  id: number;
  title: string;
  summary: string | null;
  body: string;
  author: string;
  publishedAt: string;
  isPublished: boolean;
  imageUrl: string | null;
}
