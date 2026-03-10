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
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="total"
          @size-change="loadRoles"
          @current-change="loadRoles"
        />
      </div>
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

    <el-dialog v-model="permissionDialog" title="权限配置" width="500px">
      <div class="permission-content">
        <p class="permission-tip">请为角色 "{{ currentRoleName }}" 分配菜单权限</p>
        <el-tree 
          :data="menus" 
          show-checkbox 
          node-key="id" 
          :default-checked-keys="checkedMenus"
          :default-expanded-keys="expandedMenuIds"
          ref="treeRef"
          :props="{ children: 'children', label: 'name' }"
        />
      </div>
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
import { roleApi, menuApi, Menu } from '@/api/role';

const loading = ref(false);
const roles = ref([]);
const menus = ref<Menu[]>([]);
const dialogVisible = ref(false);
const permissionDialog = ref(false);
const isEdit = ref(false);
const currentRoleId = ref(0);
const currentRoleName = ref('');
const checkedMenus = ref<number[]>([]);
const expandedMenuIds = ref<number[]>([]);
const formRef = ref();
const treeRef = ref();
const currentPage = ref(1);
const pageSize = ref(10);
const total = ref(0);

const form = reactive({ id: 0, name: '', description: '' });

const loadRoles = async () => {
  loading.value = true;
  try {
    const response = await roleApi.getPage(currentPage.value, pageSize.value);
    roles.value = response.items || [];
    total.value = response.total || 0;
  } catch (error) {
    ElMessage.error('加载角色列表失败');
  } finally {
    loading.value = false;
  }
};

const loadMenus = async () => {
  try {
    menus.value = await menuApi.getTree();
    // 获取所有菜单 ID 用于展开
    const getAllIds = (nodes: Menu[]): number[] => {
      let ids: number[] = [];
      nodes.forEach(node => {
        ids.push(node.id);
        if ((node as any).children) {
          ids = ids.concat(getAllIds((node as any).children));
        }
      });
      return ids;
    };
    expandedMenuIds.value = getAllIds(menus.value);
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
  currentRoleName.value = row.name;
  checkedMenus.value = [];
  permissionDialog.value = true;
  
  await loadMenus();
  
  // 加载该角色已分配的菜单
  try {
    const assignedMenus = await roleApi.getMenus(row.id);
    checkedMenus.value = assignedMenus.map(m => m.id);
  } catch (error) {
    console.error('加载角色菜单失败', error);
  }
};

const handleSavePermission = async () => {
  const checkedKeys = treeRef.value?.getCheckedKeys() || [];
  const halfCheckedKeys = treeRef.value?.getHalfCheckedKeys() || [];
  // 合并全选和半选的节点
  const allCheckedKeys = [...checkedKeys, ...halfCheckedKeys];
  
  try {
    await roleApi.assignMenus(currentRoleId.value, allCheckedKeys);
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
@import '../common-styles.css';

.role-list {
  animation: fadeIn 0.4s ease-out;
}

.permission-content {
  padding: 10px 0;
}

.permission-tip {
  color: #909399;
  margin-bottom: 15px;
  font-size: 14px;
  padding: 8px 12px;
  background: rgba(64, 158, 255, 0.05);
  border-radius: 6px;
  border-left: 3px solid #409eff;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
