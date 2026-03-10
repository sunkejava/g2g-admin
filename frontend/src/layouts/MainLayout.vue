<template>
  <el-container class="layout">
    <el-aside width="200px" class="aside">
      <div class="logo">G2G Admin</div>
      <el-menu 
        :default-active="activeMenu" 
        router 
        background-color="#304156" 
        text-color="#bfcbd9" 
        active-text-color="#409EFF"
        :unique-opened="true"
      >
        <!-- 根据用户权限动态渲染菜单 -->
        <template v-if="userMenus.length > 0">
          <el-menu-item 
            v-for="menu in visibleMenus" 
            :key="menu.id" 
            :index="menu.path"
          >
            <el-icon><component :is="menu.icon || 'Menu'" /></el-icon>
            <span>{{ menu.name }}</span>
          </el-menu-item>
        </template>
        <!-- 如果没有菜单数据，显示所有菜单（兼容模式） -->
        <template v-else>
          <el-menu-item index="/dashboard">
            <el-icon><UserFilled /></el-icon>
            <span>首页</span>
          </el-menu-item>
          <el-menu-item index="/users">
            <el-icon><User /></el-icon>
            <span>用户管理</span>
          </el-menu-item>
          <el-menu-item index="/roles">
            <el-icon><Lock /></el-icon>
            <span>角色管理</span>
          </el-menu-item>
          <el-menu-item index="/versions">
            <el-icon><Upload /></el-icon>
            <span>版本管理</span>
          </el-menu-item>
          <el-menu-item index="/logs">
            <el-icon><Document /></el-icon>
            <span>日志管理</span>
          </el-menu-item>
          <el-menu-item index="/settings">
            <el-icon><Setting /></el-icon>
            <span>系统配置</span>
          </el-menu-item>
          <el-menu-item index="/monitor">
            <el-icon><Monitor /></el-icon>
            <span>监控面板</span>
          </el-menu-item>
        </template>
      </el-menu>
    </el-aside>
    <el-container>
      <el-header class="header">
        <div class="header-left">G2G 后台管理系统</div>
        <div class="header-right">
          <ThemeSwitcher />
          <el-icon :size="20"><User /></el-icon>
          <span>{{ user?.username }}</span>
          <el-button link type="danger" @click="handleLogout">退出</el-button>
        </div>
      </el-header>
      <el-main class="main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { UserFilled, Lock, Upload, Document, Setting, Monitor, User } from '@element-plus/icons-vue';
import { ElMessageBox } from 'element-plus';
import ThemeSwitcher from '@/components/ThemeSwitcher.vue';

const route = useRoute();
const router = useRouter();

const userMenus = ref<any[]>([]);

const activeMenu = computed(() => route.path);
const user = computed(() => JSON.parse(localStorage.getItem('user') || '{}'));

// 根据用户菜单权限过滤可见菜单
const visibleMenus = computed(() => {
  if (userMenus.value.length === 0) return [];
  
  // 定义菜单映射关系
  const menuMap: Record<string, any> = {
    'dashboard': { id: 1, name: '首页', path: '/dashboard', icon: 'UserFilled' },
    'users': { id: 2, name: '用户管理', path: '/users', icon: 'User' },
    'roles': { id: 3, name: '角色管理', path: '/roles', icon: 'Lock' },
    'versions': { id: 4, name: '版本管理', path: '/versions', icon: 'Upload' },
    'logs': { id: 5, name: '日志管理', path: '/logs', icon: 'Document' },
    'settings': { id: 6, name: '系统配置', path: '/settings', icon: 'Setting' },
    'monitor': { id: 7, name: '监控面板', path: '/monitor', icon: 'Monitor' },
  };
  
  // 过滤出用户有权限的菜单
  return userMenus.value
    .map((menu: any) => menuMap[menu.code])
    .filter(Boolean);
});

const handleLogout = async () => {
  await ElMessageBox.confirm('确认退出登录？', '提示', { type: 'warning' });
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  localStorage.removeItem('userMenus');
  router.push('/login');
};

onMounted(() => {
  // 加载用户菜单
  const menus = localStorage.getItem('userMenus');
  if (menus) {
    userMenus.value = JSON.parse(menus);
  }
});
</script>

<style scoped>
.layout {
  height: 100vh;
}

.aside {
  background-color: #304156;
}

.logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 20px;
  font-weight: bold;
  background-color: #2b3a4b;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #fff;
  border-bottom: 1px solid #e6e6e6;
  padding: 0 20px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 15px;
}

.main {
  background-color: #f0f2f5;
  padding: 20px;
}
</style>
