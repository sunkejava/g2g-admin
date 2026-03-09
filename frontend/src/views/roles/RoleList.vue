<template>
  <div class="role-list">
    <el-card>
      <div class="toolbar">
        <el-button type="primary" @click="handleAdd">新增角色</el-button>
      </div>
      <el-table :data="roles" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="name" label="角色名" />
        <el-table-column prop="description" label="描述" />
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
            <el-button link type="warning" @click="handlePermission(row)">权限配置</el-button>
            <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑角色' : '新增角色'" width="400px">
      <el-form :model="form" ref="formRef" label-width="60px">
        <el-form-item label="角色名">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="permissionDialog" title="权限配置" width="400px">
      <el-tree :data="menus" show-checkbox node-key="id" :default-checked-keys="checkedMenus" ref="treeRef" />
      <template #footer>
        <el-button @click="permissionDialog = false">取消</el-button>
        <el-button type="primary" @click="handleSavePermission">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { roleApi, menuApi } from '@/api/role';

const loading = ref(false);
const roles = ref([]);
const menus = ref([]);
const dialogVisible = ref(false);
const permissionDialog = ref(false);
const isEdit = ref(false);
const currentRoleId = ref(0);
const checkedMenus = ref<number[]>([]);
const formRef = ref();
const treeRef = ref();

const form = reactive({ id: 0, name: '', description: '' });

const loadRoles = async () => {
  loading.value = true;
  try {
    roles.value = await roleApi.getAll();
  } catch (error) {
    ElMessage.error('加载角色列表失败');
  } finally {
    loading.value = false;
  }
};

const loadMenus = async () => {
  try {
    menus.value = await menuApi.getTree();
  } catch (error) {
    console.error('加载菜单失败', error);
  }
};

const handleAdd = () => {
  isEdit.value = false;
  Object.assign(form, { id: 0, name: '', description: '' });
  dialogVisible.value = true;
};

const handleEdit = (row: any) => {
  isEdit.value = true;
  Object.assign(form, { id: row.id, name: row.name, description: row.description });
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  try {
    if (isEdit.value) {
      await roleApi.update(form.id, { name: form.name, description: form.description });
      ElMessage.success('更新成功');
    } else {
      await roleApi.create({ name: form.name, description: form.description });
      ElMessage.success('创建成功');
    }
    dialogVisible.value = false;
    loadRoles();
  } catch (error) {
    ElMessage.error('操作失败');
  }
};

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm('确认删除该角色？', '警告', { type: 'warning' });
  try {
    await roleApi.delete(row.id);
    ElMessage.success('删除成功');
    loadRoles();
  } catch (error) {
    ElMessage.error('删除失败');
  }
};

const handlePermission = async (row: any) => {
  currentRoleId.value = row.id;
  checkedMenus.value = [];
  permissionDialog.value = true;
  await loadMenus();
};

const handleSavePermission = async () => {
  const checkedKeys = treeRef.value?.getCheckedKeys() || [];
  try {
    await roleApi.assignMenus(currentRoleId.value, checkedKeys);
    ElMessage.success('权限配置成功');
    permissionDialog.value = false;
  } catch (error) {
    ElMessage.error('配置失败');
  }
};

onMounted(() => {
  loadRoles();
});
</script>

<style scoped>
.toolbar {
  margin-bottom: 20px;
}
</style>
