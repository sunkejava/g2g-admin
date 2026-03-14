<template>
  <el-container class="layout">
    <el-aside width="200px" class="aside">
      <div class="logo">
        <img v-if="systemConfig.icon" :src="systemConfig.icon" alt="Logo" style="height: 32px; margin-right: 10px" />
        {{ systemConfig.name }}
      </div>
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
        <div class="header-left">{{ systemConfig.fullName }}</div>
        <div class="header-right">
          <ThemeSwitcher />
          <el-icon :size="20"><User /></el-icon>
          <span>{{ user?.username }}</span>
          <el-button link type="danger" @click="handleLogout">退出</el-button>
        </div>
      </el-header>
      <el-main class="main">
        <router-view />
        <div class="copyright">
          <p>{{ systemConfig.copyright }} {{ systemConfig.support ? '· ' + systemConfig.support : '' }}</p>
        </div>
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { UserFilled, Lock, Upload, Document, Setting, Monitor, User } from '@element-plus/icons-vue';
import { ElMessageBox } from 'element-plus';
import ThemeSwitcher from '@/components/ThemeSwitcher.vue';
import api from '@/api/request';

const route = useRoute();
const router = useRouter();

const userMenus = ref<any[]>([]);

// 系统配置
const systemConfig = reactive({
  name: 'G2G Admin',
  fullName: 'G2G 后台管理系统',
  icon: '/g2g-logo.svg',
  version: '1.1.0',
  copyright: '© 2026 G2G Team',
  support: ''
});

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

const loadSystemConfig = async () => {
  try {
    const settings = await api.get('/settings');
    const basicSetting = settings.find((s: any) => s.key === 'System.Basic');
    if (basicSetting?.value) {
      const config = JSON.parse(basicSetting.value);
      if (config.systemName) {
        systemConfig.name = config.systemName;
        systemConfig.fullName = config.systemName;
      }
      if (config.systemIcon) systemConfig.icon = config.systemIcon;
      if (config.version) systemConfig.version = config.version;
      if (config.copyright) systemConfig.copyright = config.copyright;
      if (config.support) systemConfig.support = config.support;
      
      // 更新页面标题
      document.title = systemConfig.fullName;
    }
  } catch (error) {
    console.error('加载系统配置失败', error);
  }
};

onMounted(() => {
  // 加载用户菜单
  const menus = localStorage.getItem('userMenus');
  if (menus) {
    userMenus.value = JSON.parse(menus);
  }
  // 加载系统配置
  loadSystemConfig();
});
</script>

<style scoped>
.layout {
  height: 100vh;
  overflow: hidden;
}

.aside {
  background: linear-gradient(180deg, #1a1c2e 0%, #2d3142 100%);
  box-shadow: 4px 0 24px rgba(0, 0, 0, 0.15);
}

.logo {
  height: 70px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 22px;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
  letter-spacing: 2px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9ff 100%);
  border-bottom: 1px solid rgba(102, 126, 234, 0.1);
  padding: 0 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.header-left {
  font-size: 18px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.main {
  background: linear-gradient(135deg, #f5f7fa 0%, #e8ecff 100%);
  padding: 24px;
  position: relative;
  min-height: calc(100vh - 70px);
  overflow-y: auto;
}

.copyright {
  position: absolute;
  bottom: 20px;
  left: 0;
  right: 0;
  text-align: center;
  color: #909399;
  font-size: 12px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  border-radius: 8px;
  margin: 0 24px;
}

.copyright p {
  margin: 0;
  letter-spacing: 0.5px;
}

/* 侧边栏菜单项动画 */
:deep(.el-menu-item) {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.el-menu-item:hover) {
  transform: translateX(4px);
}

:deep(.el-menu-item.is-active) {
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}
</style>
