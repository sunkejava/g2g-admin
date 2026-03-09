import api from './request';

export interface LoginParams {
  username: string;
  password: string;
}

export interface UserInfo {
  id: number;
  username: string;
  email: string;
}

export interface LoginResponse {
  token: string;
  user: UserInfo;
}

export const authApi = {
  login: (data: LoginParams) => api.post<LoginResponse>('/auth/login', data),
  logout: () => api.post('/auth/logout'),
};
