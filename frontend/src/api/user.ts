import api from './request';

export interface User {
  id: number;
  username: string;
  email: string;
  phone?: string;
  roles: Role[];
  status: boolean;
  createdAt: string;
}

export interface Role {
  id: number;
  name: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const userApi = {
  getAll: (page: number = 1, pageSize: number = 100) => 
    api.get<PagedResult<User>>(`/users?page=${page}&pageSize=${pageSize}`),
  
  getPage: (page: number = 1, pageSize: number = 10, keyword?: string) => {
    let url = `/users?page=${page}&pageSize=${pageSize}`;
    if (keyword) url += `&keyword=${keyword}`;
    return api.get<PagedResult<User>>(url);
  },
  
  getById: (id: number) => api.get<User>(`/users/${id}`),
  create: (data: { username: string; email: string; phone?: string; password: string; roleIds: number[] }) => 
    api.post<User>('/users', data),
  update: (id: number, data: { email?: string; phone?: string; roleIds?: number[] }) => 
    api.put<User>(`/users/${id}`, data),
  delete: (id: number) => api.delete(`/users/${id}`),
  resetPassword: (id: number, newPassword: string) => 
    api.post(`/users/${id}/reset-password`, { newPassword }),
  toggleStatus: (id: number) => api.post(`/users/${id}/toggle-status`),
};
