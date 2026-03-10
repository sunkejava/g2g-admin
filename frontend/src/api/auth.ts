import api from './request';

export interface LoginResponse {
  token: string;
  user: {
    id: number;
    username: string;
    email: string;
    roles: { id: number; name: string }[];
  };
}

export const authApi = {
  login: (username: string, password: string) => 
    api.post<LoginResponse>('/auth/login', { username, password }),
  
  logout: () => api.post('/auth/logout', {}),
};
