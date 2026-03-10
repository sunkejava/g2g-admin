<template>
  <div class="user-list">
    <el-card>
      <div class="toolbar">
        <el-button type="primary" @click="handleAdd">新增用户</el-button>
        <el-input 
          v-model="keyword" 
          placeholder="搜索用户名/邮箱" 
          style="width: 200px; margin-left: 10px" 
          clearable 
          @clear="loadUsers" 
          @keyup.enter="loadUsers" 
        />
        <el-button @click="loadUsers" style="margin-left: 10px">搜索</el-button>
      </div>
      <el-table :data="users" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="username" label="用户名" />
        <el-table-column prop="email" label="邮箱" />
        <el-table-column prop="phone" label="手机" />
        <el-table-column label="角色" width="150">
          <template #default="{ row }">
            <el-tag v-for="role in row.roles" :key="role.id" size="small" style="margin-right: 5px">{{ role.name }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-switch v-model="row.status" :active-value="true" :inactive-value="false" @change="handleToggleStatus(row)" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
            <el-button link type="warning" @click="handleResetPassword(row)">重置密码</el-button>
            <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="total"
          @size-change="loadUsers"
          @current-change="loadUsers"
        />
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑用户' : '新增用户'" width="500px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="80px">
        <el-form-item label="用户名" prop="username" v-if="!isEdit">
          <el-input v-model="form.username" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="手机">
          <el-input v-model="form.phone" />
        </el-form-item>
        <el-form-item label="密码" prop="password" v-if="!isEdit">
          <el-input v-model="form.password" type="password" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select 
            v-model="form.roleIds" 
            multiple 
            placeholder="请选择角色"
            style="width: 100%"
            :disabled="roles.length === 0"
          >
            <el-option 
              v-for="role in roles" 
              :key="role.id" 
              :label="role.name" 
              :value="role.id"
            />
          </el-select>
          <div v-if="roles.length === 0" style="color: #999; font-size: 12px; margin-top: 5px">
            加载中...
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { userApi } from '@/api/user';
import { roleApi } from '@/api/role';

const loading = ref(false);
const users = ref([]);
const roles = ref([]);
const keyword = ref('');
const dialogVisible = ref(false);
const isEdit = ref(false);
const formRef = ref();
const currentPage = ref(1);
const pageSize = ref(10);
const total = ref(0);

const form = reactive({ id: 0, username: '', email: '', phone: '', password: '', roleIds: [] as number[] });
const rules = { 
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }], 
  email: [{ required: true, message: '请输入邮箱', trigger: 'blur' }], 
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }] 
};

const loadUsers = async () => {
  loading.value = true;
  try {
    const response = await userApi.getPage(currentPage.value, pageSize.value, keyword.value);
    users.value = response.items || [];
    total.value = response.total || 0;
  } catch (error) {
    ElMessage.error('加载用户列表失败');
  } finally {
    loading.value = false;
  }
};

const loadRoles = async () => {
  try {
    const data = await roleApi.getAll();
    roles.value = data;
    console.log('角色加载成功:', roles.value);
  } catch (error: any) {
    console.error('加载角色失败:', error);
    ElMessage.error('加载角色失败：' + (error.message || '未知错误'));
  }
};

const handleAdd = () => {
  isEdit.value = false;
  Object.assign(form, { id: 0, username: '', email: '', phone: '', password: '', roleIds: [] });
  dialogVisible.value = true;
};

const handleEdit = (row: any) => {
  isEdit.value = true;
  Object.assign(form, { id: row.id, username: row.username, email: row.email, phone: row.phone, password: '', roleIds: row.roles?.map((r: any) => r.id) || [] });
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  await formRef.value.validate();
  try {
    if (isEdit.value) {
      await userApi.update(form.id, { email: form.email, phone: form.phone, roleIds: form.roleIds });
      ElMessage.success('更新成功');
    } else {
      await userApi.create(form);
      ElMessage.success('创建成功');
    }
    dialogVisible.value = false;
    loadUsers();
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '操作失败');
  }
};

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm('确认删除该用户？', '警告', { type: 'warning' });
  try {
    await userApi.delete(row.id);
    ElMessage.success('删除成功');
    loadUsers();
  } catch (error) {
    ElMessage.error('删除失败');
  }
};

const handleResetPassword = async (row: any) => {
  const { value } = await ElMessageBox.prompt('请输入新密码', '重置密码', { inputPattern: /.+/, inputErrorMessage: '密码不能为空' });
  try {
    await userApi.resetPassword(row.id, value);
    ElMessage.success('密码重置成功');
  } catch (error) {
    ElMessage.error('重置失败');
  }
};

const handleToggleStatus = async (row: any) => {
  try {
    await userApi.toggleStatus(row.id);
    ElMessage.success('状态更新成功');
  } catch (error) {
    ElMessage.error('更新失败');
    row.status = !row.status;
  }
};

onMounted(() => {
  loadUsers();
  loadRoles();
});
</script>

<style scoped>
.toolbar {
  margin-bottom: 20px;
  display: flex;
  align-items: center;
}
.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
