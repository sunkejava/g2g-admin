<template>
  <div class="log-list">
    <el-card>
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <el-tab-pane label="操作日志" name="operation" />
        <el-tab-pane label="系统日志" name="system" />
        <el-tab-pane label="登录日志" name="login" />
      </el-tabs>
      <div class="toolbar">
        <el-date-picker 
          v-model="dateRange" 
          type="daterange" 
          range-separator="至" 
          start-placeholder="开始日期" 
          end-placeholder="结束日期" 
          value-format="YYYY-MM-DD"
          @change="loadLogs" 
        />
        <el-button 
          :type="!dateRange ? 'primary' : 'default'" 
          @click="clearDateFilter"
          style="margin-left: 10px"
        >
          全部时间
        </el-button>
        <el-input 
          v-model="keyword" 
          placeholder="搜索" 
          style="width: 200px; margin-left: 10px" 
          clearable 
          @clear="loadLogs" 
          @keyup.enter="loadLogs" 
        />
        <el-button @click="loadLogs" style="margin-left: 10px">搜索</el-button>
        <el-button type="success" @click="handleExport" style="margin-left: 10px">导出 Excel</el-button>
      </div>
      <div class="filter-tip" v-if="dateRange">
        <el-icon><Filter /></el-icon>
        <span>当前筛选：{{ dateRange[0] }} 至 {{ dateRange[1] }}（近 7 天数据）</span>
        <el-button link type="primary" @click="clearDateFilter">清除筛选</el-button>
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
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="total"
          @size-change="loadLogs"
          @current-change="loadLogs"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { Filter } from '@element-plus/icons-vue';
import api from '@/api/request';

const activeTab = ref('operation');
const loading = ref(false);
const logs = ref([]);
const dateRange = ref<[string, string] | null>(null);
const keyword = ref('');
const currentPage = ref(1);
const pageSize = ref(10);
const total = ref(0);

// 初始化日期范围为近一周
const initDateRange = () => {
  const now = new Date();
  const oneWeekAgo = new Date();
  oneWeekAgo.setDate(now.getDate() - 7);
  dateRange.value = [
    oneWeekAgo.toISOString().split('T')[0],
    now.toISOString().split('T')[0]
  ];
};

const loadLogs = async () => {
  loading.value = true;
  try {
    const from = dateRange.value?.[0] ? new Date(dateRange.value[0]).toISOString() : '';
    const to = dateRange.value?.[1] ? new Date(dateRange.value[1]).toISOString() : '';
    const url = `/logs/${activeTab.value}?page=${currentPage.value}&pageSize=${pageSize.value}&from=${from}&to=${to}&keyword=${keyword.value}`;
    const response = await api.get(url);
    logs.value = response.items || [];
    total.value = response.total || 0;
  } catch (error) {
    ElMessage.error('加载日志失败');
  } finally {
    loading.value = false;
  }
};

const handleTabChange = () => {
  currentPage.value = 1;
  loadLogs();
};

const handleExport = async () => {
  const from = dateRange.value?.[0] || '';
  const to = dateRange.value?.[1] || '';
  window.open(`/api/logs/export/${activeTab.value}?from=${from}&to=${to}`, '_blank');
  ElMessage.success('导出中...');
};

const clearDateFilter = () => {
  dateRange.value = null;
  currentPage.value = 1;
  loadLogs();
};

onMounted(() => {
  initDateRange();
  loadLogs();
});
</script>

<style scoped>
.toolbar {
  margin: 20px 0;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}
.filter-tip {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 15px;
  background: #f0f9eb;
  border-radius: 4px;
  color: #67c23a;
  font-size: 14px;
  margin-bottom: 15px;
}
.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
