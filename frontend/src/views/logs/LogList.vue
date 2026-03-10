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
        <el-table-column v-if="activeTab === 'operation'" prop="createdAt" label="时间" width="200">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column v-if="activeTab === 'operation'" prop="userId" label="用户 ID" width="80" />
        <el-table-column v-if="activeTab === 'operation'" prop="action" label="操作" />
        <el-table-column v-if="activeTab === 'operation'" prop="module" label="模块" />
        <el-table-column v-if="activeTab === 'operation'" prop="details" label="详情" show-overflow-tooltip />
        <el-table-column v-if="activeTab === 'operation'" prop="ip" label="IP" width="140" />

        <el-table-column v-if="activeTab === 'system'" prop="createdAt" label="时间" width="200">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column v-if="activeTab === 'system'" prop="level" label="级别" width="80">
          <template #default="{ row }">
            <el-tag :type="row.level === 'Error' ? 'danger' : 'warning'">{{ row.level }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column v-if="activeTab === 'system'" prop="source" label="来源" />
        <el-table-column v-if="activeTab === 'system'" prop="message" label="消息" show-overflow-tooltip />

        <el-table-column v-if="activeTab === 'login'" prop="createdAt" label="时间" width="200">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
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
import { ElMessage, ElMessageBox } from 'element-plus';
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
    let from = '';
    let to = '';
    
    if (dateRange.value?.[0]) {
      // 开始日期：当天 00:00:00
      const fromDate = new Date(dateRange.value[0]);
      from = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate()).toISOString();
    }
    
    if (dateRange.value?.[1]) {
      // 结束日期：当天 23:59:59
      const toDate = new Date(dateRange.value[1]);
      to = new Date(toDate.getFullYear(), toDate.getMonth(), toDate.getDate(), 23, 59, 59, 999).toISOString();
    }
    
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
  
  // 构建导出文件名
  const tabNames: Record<string, string> = {
    operation: '操作日志',
    system: '系统日志',
    login: '登录日志'
  };
  const fileName = `${tabNames[activeTab.value]}_${from || '全部'}_${to || ''}.xlsx`;
  
  // 显示确认对话框
  await ElMessageBox.confirm(
    `确定要导出${tabNames[activeTab.value]}吗？${from && to ? `\n日期范围：${from} 至 ${to}` : '\n将导出所有数据'}`,
    '导出确认',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'info'
    }
  );
  
  try {
    // 显示加载提示
    const loadingMsg = ElMessage({
      message: '正在导出，请稍候...',
      type: 'info',
      duration: 0
    });
    
    // 构建下载 URL（带上 token）
    const token = localStorage.getItem('token');
    const exportUrl = `/api/logs/export/${activeTab.value}?from=${from}&to=${to}`;
    
    // 使用 fetch 下载，可以添加 header
    const response = await fetch(exportUrl, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      throw new Error('导出失败');
    }
    
    // 获取 blob 并下载
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName.replace(/\//g, '-');
    document.body.appendChild(link);
    link.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(link);
    
    loadingMsg.close();
    ElMessage.success('导出成功');
  } catch (error: any) {
    ElMessage.error(error.message || '导出失败');
  }
};

const clearDateFilter = () => {
  dateRange.value = null;
  currentPage.value = 1;
  loadLogs();
};

// 格式化时间为 yyyy-MM-dd HH:mm:ss fff
const formatDateTime = (dateTime: string) => {
  if (!dateTime) return '';
  const date = new Date(dateTime);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  const milliseconds = String(date.getMilliseconds()).padStart(3, '0');
  
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds} ${milliseconds}`;
};

onMounted(() => {
  initDateRange();
  loadLogs();
});
</script>

<style scoped>
@import '../common-styles.css';

.log-list {
  animation: fadeIn 0.4s ease-out;
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
