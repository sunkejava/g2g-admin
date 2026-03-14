import { createRouter, createWebHistory } from 'vue-router';
import MainLayout from '@/layouts/MainLayout.vue';

// 公开路由（无需登录）
const publicRoutes = [
  { path: '/login', component: () => import('@/views/Login.vue') },
  // { path: '/register', component: () => import('@/views/Register.vue') }, // 注册功能已关闭
  { path: '/403', component: () => import('@/views/403.vue') },
];

// 基础路由结构（需要登录）
const baseRoutes = [
  {
    path: '/',
    component: MainLayout,
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', component: () => import('@/views/Dashboard.vue'), meta: { code: 'dashboard' } },
      { path: 'users', component: () => import('@/views/users/UserList.vue'), meta: { code: 'users' } },
      { path: 'roles', component: () => import('@/views/roles/RoleList.vue'), meta: { code: 'roles' } },
      { path: 'versions', component: () => import('@/views/versions/VersionList.vue'), meta: { code: 'versions' } },
      { path: 'logs', component: () => import('@/views/logs/LogList.vue'), meta: { code: 'logs' } },
      { path: 'settings', component: () => import('@/views/settings/SystemSettings.vue'), meta: { code: 'settings' } },
      { path: 'monitor', component: () => import('@/views/monitor/SystemMonitor.vue'), meta: { code: 'monitor' } },
    ],
  },
];

const router = createRouter({
  history: createWebHistory('/'),
  routes: [...publicRoutes, ...baseRoutes],
});

// 路由守卫
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  
  // 公开页面直接访问
  if (['/login'].includes(to.path)) {
    if (token) {
      next('/');
    } else {
      next();
    }
    return;
  }
  
  // 需要登录的页面
  if (!token) {
    next('/login');
    return;
  }
  
  // 检查菜单权限（如果有用户菜单数据）
  const userMenus = JSON.parse(localStorage.getItem('userMenus') || '[]');
  if (userMenus.length > 0 && to.meta?.code) {
    const hasPermission = userMenus.some((menu: any) => menu.code === to.meta?.code);
    if (!hasPermission) {
      // 没有权限，重定向到 403 或首页
      next({ path: '/403', replace: true });
      return;
    }
  }
  
  next();
});

export default router;
