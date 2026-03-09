import api from './request';

export interface User {
  id: number;
  username: string;
  email: string;
  phone?: string;
  status: boolean;
  createdAt: string;
  roles?: { id: number; name: string }[];
}

export interface CreateUserDto {
  username: string;
  email: string;
  phone?: string;
  password: string;
  roleIds: number[];
}

export interface UpdateUserDto {
  email?: string;
  phone?: string;
  roleIds: number[];
}

export const userApi = {
  getAll: (page: number, pageSize: number) => api.get<User[]>(`/users?page=${page}&pageSize=${pageSize}`),
  getById: (id: number) => api.get<User>(`/users/${id}`),
  create: (data: CreateUserDto) => api.post<User>('/users', data),
  update: (id: number, data: UpdateUserDto) => api.put<User>(`/users/${id}`, data),
  delete: (id: number) => api.delete(`/users/${id}`),
  resetPassword: (id: number, newPassword: string) => api.post(`/users/${id}/reset-password`, { newPassword }),
  toggleStatus: (id: number) => api.patch(`/users/${id}/toggle-status`),
};
