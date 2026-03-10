<template>
  <div class="log-list">
    <el-card>
      <el-tabs v-model="activeTab">
        <el-tab-pane label="操作日志" name="operation" />
        <el-tab-pane label="系统日志" name="system" />
        <el-tab-pane label="登录日志" name="login" />
      </el-tabs>
      <div class="toolbar">
        <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" @change="loadLogs" />
        <el-input v-model="keyword" placeholder="搜索" style="width: 200px; margin-left: 10px" clearable @clear="loadLogs" />
        <el-button @click="loadLogs" style="margin-left: 10px">搜索</el-button>
        <el-button type="success" @click="handleExport" style="margin-left: 10px">导出 Excel</el-button>
      </div>
      <el-table :data="logs" style="width: 100%" v-loading="loading">
        <el-table-column v-if="activeTab === 'operation'" prop="createdAt" label="时间" width="180" />
        <el-table-column v-if="activeTab === 'operation'" prop="userId" label="用户 ID" width="80" />
        <el-table-column v-if="activeTab === 'operation'" prop="action" label="操作" />
        <el-table-column v-if="activeTab === 'operation'" prop="module" label="模块" />
        <el-table-column v-if="activeTab === 'operation'" prop="details" label="详情" show-overflow-tooltip />
        <el-table-column v-if="activeTab === 'operation'" prop="ip" label="IP" width="140" />

        <el-table-column v-if="activeTab === 'system'" prop="createdAt" label="时间" width="180" />
        <el-table-column v-if="activeTab === 'system'" prop="level" label="级别" width="80">
          <template #default="{ row }">
            <el-tag :type="row.level === 'Error' ? 'danger' : 'warning'">{{ row.level }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column v-if="activeTab === 'system'" prop="source" label="来源" />
        <el-table-column v-if="activeTab === 'system'" prop="message" label="消息" show-overflow-tooltip />

        <el-table-column v-if="activeTab === 'login'" prop="createdAt" label="时间" width="180" />
        <el-table-column v-if="activeTab === 'login'" prop="username" label="用户名" />
        <el-table-column v-if="activeTab === 'login'" prop="success" label="结果" width="80">
          <template #default="{ row }">
            <el-tag :type="row.success ? 'success' : 'danger'">{{ row.success ? '成功' : '失败' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column v-if="activeTab === 'login'" prop="ip" label="IP" width="140" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import api from '@/api/request';

const activeTab = ref('operation');
const loading = ref(false);
const logs = ref([]);
const dateRange = ref<[Date, Date] | null>(null);
const keyword = ref('');

const loadLogs = async () => {
  loading.value = true;
  try {
    const from = dateRange.value?.[0]?.toISOString();
    const to = dateRange.value?.[1]?.toISOString();
    const url = `/logs/${activeTab.value}?from=${from || ''}&to=${to || ''}&keyword=${keyword.value}`;
    logs.value = (await api.get(url)).items || [];
  } catch (error) {
    ElMessage.error('加载日志失败');
  } finally {
    loading.value = false;
  }
};

const handleExport = async () => {
  const from = dateRange.value?.[0]?.toISOString() || '';
  const to = dateRange.value?.[1]?.toISOString() || '';
  window.open(`/api/logs/export/${activeTab.value}?from=${from}&to=${to}`, '_blank');
  ElMessage.success('导出中...');
};

onMounted(() => {
  loadLogs();
});
</script>

<style scoped>
.toolbar {
  margin: 20px 0;
  display: flex;
  align-items: center;
}
</style>
