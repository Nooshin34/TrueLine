export interface AuthSession {
  token: string;
  name: string;
  email: string;
  hasAvatar: boolean;
  avatarUrl: string | null;
  role: 'reporter' | 'admin';
}

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  hasAvatar: boolean;
  role: 'reporter' | 'admin';
}
