<template>
  <div class="version-list">
    <el-card>
      <div class="toolbar">
        <el-button type="primary" @click="uploadDialog = true">上传版本</el-button>
      </div>
      <el-table :data="versions" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="versionNo" label="版本号" />
        <el-table-column prop="fileSize" label="文件大小">
          <template #default="{ row }">{{ (row.fileSize / 1024 / 1024).toFixed(2) }} MB</template>
        </el-table-column>
        <el-table-column prop="releaseNotes" label="更新说明" show-overflow-tooltip />
        <el-table-column label="当前版本" width="80">
          <template #default="{ row }">
            <el-tag v-if="row.isCurrent" type="success">是</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="uploadedAt" label="上传时间" width="180" />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link type="warning" @click="handleCompare(row)">对比</el-button>
            <el-button link type="danger" @click="handleRollback(row)" :disabled="row.isCurrent">回滚</el-button>
            <el-button link type="danger" @click="handleDelete(row)" :disabled="row.isCurrent">删除</el-button>
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
          @size-change="loadVersions"
          @current-change="loadVersions"
        />
      </div>
    </el-card>

    <el-dialog v-model="uploadDialog" title="上传版本" width="500px">
      <el-form :model="uploadForm" label-width="80px">
        <el-form-item label="版本号">
          <el-input v-model="uploadForm.versionNo" placeholder="如：1.0.0" />
        </el-form-item>
        <el-form-item label="更新说明">
          <el-input v-model="uploadForm.releaseNotes" type="textarea" />
        </el-form-item>
        <el-form-item label="安装包">
          <el-upload 
            drag 
            action="#" 
            :auto-upload="false" 
            :on-change="handleFileChange" 
            :limit="1"
          >
            <el-icon><Upload /></el-icon>
            <div class="el-upload__text">拖拽文件到此处或<em>点击上传</em></div>
          </el-upload>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="uploadDialog = false">取消</el-button>
        <el-button type="primary" @click="handleUpload" :loading="uploading">上传</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import api from '@/api/request';

const loading = ref(false);
const versions = ref([]);
const uploadDialog = ref(false);
const uploading = ref(false);
const selectedFile = ref<File | null>(null);
const uploadForm = reactive({ versionNo: '', releaseNotes: '' });
const currentPage = ref(1);
const pageSize = ref(10);
const total = ref(0);

const loadVersions = async () => {
  loading.value = true;
  try {
    const response = await api.get(`/versions?page=${currentPage.value}&pageSize=${pageSize.value}`);
    versions.value = response.items || [];
    total.value = response.total || 0;
  } catch (error) {
    ElMessage.error('加载版本列表失败');
  } finally {
    loading.value = false;
  }
};

const handleFileChange = (file: any) => {
  selectedFile.value = file.raw;
};

const handleUpload = async () => {
  if (!selectedFile.value || !uploadForm.versionNo) {
    ElMessage.warning('请选择文件和填写版本号');
    return;
  }
  uploading.value = true;
  const formData = new FormData();
  formData.append('file', selectedFile.value);
  formData.append('versionNo', uploadForm.versionNo);
  formData.append('releaseNotes', uploadForm.releaseNotes);
  try {
    await api.post('/versions/upload', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
    ElMessage.success('上传成功');
    uploadDialog.value = false;
    loadVersions();
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '上传失败');
  } finally {
    uploading.value = false;
  }
};

const handleRollback = async (row: any) => {
  await ElMessageBox.confirm(`确认回滚到版本 ${row.versionNo}？`, '警告', { type: 'warning' });
  try {
    await api.post(`/versions/${row.id}/rollback`);
    ElMessage.success('回滚成功');
    loadVersions();
  } catch (error) {
    ElMessage.error('回滚失败');
  }
};

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(`确认删除版本 ${row.versionNo}？`, '警告', { type: 'warning' });
  try {
    await api.delete(`/versions/${row.id}`);
    ElMessage.success('删除成功');
    loadVersions();
  } catch (error) {
    ElMessage.error('删除失败');
  }
};

const handleCompare = (row: any) => {
  ElMessage.info('对比功能开发中...');
};

onMounted(() => {
  loadVersions();
});
</script>

<style scoped>
.toolbar {
  margin-bottom: 20px;
}
.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
