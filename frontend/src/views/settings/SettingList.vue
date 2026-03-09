<template>
  <div class="setting-list">
    <el-card>
      <el-table :data="settings" style="width: 100%" v-loading="loading">
        <el-table-column prop="key" label="配置键" />
        <el-table-column prop="value" label="配置值" />
        <el-table-column prop="description" label="描述" />
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 20px">
        <el-button type="warning" @click="handleClearCache">清除缓存</el-button>
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" title="编辑配置" width="400px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="配置键">
          <el-input v-model="form.key" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="配置值">
          <el-input v-model="form.value" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import api from '@/api/request';

const loading = ref(false);
const settings = ref([]);
const dialogVisible = ref(false);
const isEdit = ref(false);
const form = reactive({ key: '', value: '', description: '' });

const loadSettings = async () => {
  loading.value = true;
  try {
    settings.value = await api.get('/settings');
  } catch (error) {
    ElMessage.error('加载配置失败');
  } finally {
    loading.value = false;
  }
};

const handleEdit = (row: any) => {
  isEdit.value = true;
  Object.assign(form, { key: row.key, value: row.value, description: row.description });
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  try {
    await api.put(`/settings/${form.key}`, { value: form.value, description: form.description });
    ElMessage.success('保存成功');
    dialogVisible.value = false;
    loadSettings();
  } catch (error) {
    ElMessage.error('保存失败');
  }
};

const handleDelete = async (row: any) => {
  try {
    await api.delete(`/settings/${row.key}`);
    ElMessage.success('删除成功');
    loadSettings();
  } catch (error) {
    ElMessage.error('删除失败');
  }
};

const handleClearCache = async () => {
  try {
    await api.post('/settings/cache/clear');
    ElMessage.success('缓存已清除');
  } catch (error) {
    ElMessage.error('清除失败');
  }
};

onMounted(() => {
  loadSettings();
});
</script>
