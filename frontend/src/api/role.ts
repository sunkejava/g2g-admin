import api from './request';

export interface Role {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
}

export interface Menu {
  id: number;
  code: string;
  name: string;
  path?: string;
  icon?: string;
  parentId?: number;
  sort: number;
  children?: Menu[];
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const roleApi = {
  getAll: () => api.get<Role[]>('/roles'),
  
  getPage: (page: number = 1, pageSize: number = 10) => 
    api.get<PagedResult<Role>>(`/roles?page=${page}&pageSize=${pageSize}`),
  
  getById: (id: number) => api.get<Role>(`/roles/${id}`),
  create: (data: { name: string; description?: string }) => api.post<Role>('/roles', data),
  update: (id: number, data: { name?: string; description?: string }) => api.put<Role>(`/roles/${id}`, data),
  delete: (id: number) => api.delete(`/roles/${id}`),
  assignMenus: (id: number, menuIds: number[]) => api.post(`/roles/${id}/menus`, menuIds),
  getMenus: (id: number) => api.get<Menu[]>(`/roles/${id}/menus`),
};

export const menuApi = {
  getAll: () => api.get<Menu[]>('/menus'),
  getTree: () => api.get<Menu[]>('/menus/tree'),
  getMyMenus: () => api.get<Menu[]>('/menus/my'),
};
