<template>
  <div class="login-container">
    <el-card class="login-card">
      <h2 class="login-title">G2G 后台管理系统</h2>
      <el-form :model="form" :rules="rules" ref="formRef" @keyup.enter="handleLogin">
        <el-form-item prop="username">
          <el-input v-model="form.username" placeholder="用户名" prefix-icon="User" />
        </el-form-item>
        <el-form-item prop="password">
          <el-input v-model="form.password" type="password" placeholder="密码" prefix-icon="Lock" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleLogin" :loading="loading" style="width: 100%">登录</el-button>
        </el-form-item>
      </el-form>
      <div class="login-footer">
        <el-text type="info" size="small">
          ℹ️ 注册功能已关闭，如需创建账号请联系管理员
        </el-text>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { authApi } from '@/api/auth';
import { menuApi } from '@/api/role';

const router = useRouter();
const loading = ref(false);
const formRef = ref();

const form = reactive({
  username: '',
  password: ''
});

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
};

const handleLogin = async () => {
  await formRef.value.validate();
  loading.value = true;
  
  try {
    // 登录
    const result = await authApi.login(form.username, form.password);
    
    // 保存 token 和用户信息
    localStorage.setItem('token', result.token);
    localStorage.setItem('user', JSON.stringify(result.user));
    
    // 获取用户菜单权限
    try {
      const menus = await menuApi.getMyMenus();
      localStorage.setItem('userMenus', JSON.stringify(menus));
      console.log('用户菜单权限:', menus);
    } catch (error) {
      console.error('获取菜单权限失败:', error);
      // 如果获取失败，给一个默认的空数组
      localStorage.setItem('userMenus', '[]');
    }
    
    ElMessage.success('登录成功');
    router.push('/');
  } catch (error: any) {
    ElMessage({
      message: error.response?.data?.message || '登录失败',
      type: 'error',
      duration: 2000 // 2 秒后自动关闭
    });
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
  overflow: hidden;
}

.login-container::before {
  content: '';
  position: absolute;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 60%);
  animation: rotate 20s linear infinite;
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.login-card {
  width: 420px;
  padding: 40px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.5s ease-out;
  position: relative;
  z-index: 1;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.login-title {
  text-align: center;
  margin-bottom: 40px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  font-size: 28px;
  font-weight: 700;
  letter-spacing: 1px;
}

.login-footer {
  text-align: center;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}

/* 输入框动画 */
:deep(.el-form-item) {
  animation: fadeIn 0.3s ease-out;
  animation-fill-mode: both;
}

:deep(.el-form-item:nth-child(1)) {
  animation-delay: 0.1s;
}

:deep(.el-form-item:nth-child(2)) {
  animation-delay: 0.2s;
}

:deep(.el-form-item:nth-child(3)) {
  animation-delay: 0.3s;
}

:deep(.el-form-item:nth-child(4)) {
  animation-delay: 0.4s;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>
