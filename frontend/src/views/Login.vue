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
        <el-link type="primary" @click="$router.push('/register')">注册账号</el-link>
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
  username: 'admin',
  password: 'admin123'
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
    ElMessage.error(error.response?.data?.message || '登录失败');
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
}

.login-card {
  width: 400px;
  padding: 20px;
}

.login-title {
  text-align: center;
  margin-bottom: 30px;
  color: #303133;
  font-size: 24px;
}

.login-footer {
  text-align: center;
  margin-top: 15px;
}
</style>
