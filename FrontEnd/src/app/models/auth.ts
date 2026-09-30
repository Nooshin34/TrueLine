export interface AuthSession {
  token: string;
  name: string;
  email: string;
  hasAvatar: boolean;
  avatarUrl: string | null;
}

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  hasAvatar: boolean;
}
