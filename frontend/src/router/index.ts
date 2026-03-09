import { createRouter, createWebHistory } from 'vue-router';
import MainLayout from '@/layouts/MainLayout.vue';

const routes = [
  { path: '/login', component: () => import('@/views/Login.vue') },
  { path: '/register', component: () => import('@/views/Register.vue') },
  {
    path: '/',
    component: MainLayout,
    redirect: '/dashboard',
    children: [
      { path: 'dashboard', component: () => import('@/views/Dashboard.vue') },
      { path: 'users', component: () => import('@/views/users/UserList.vue') },
      { path: 'roles', component: () => import('@/views/roles/RoleList.vue') },
      { path: 'versions', component: () => import('@/views/versions/VersionList.vue') },
      { path: 'logs', component: () => import('@/views/logs/LogList.vue') },
      { path: 'settings', component: () => import('@/views/settings/SettingList.vue') },
      { path: 'monitor', component: () => import('@/views/monitor/SystemMonitor.vue') },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  if (to.path !== '/login' && !token) {
    next('/login');
  } else if (to.path === '/login' && token) {
    next('/');
  } else {
    next();
  }
});

export default router;
