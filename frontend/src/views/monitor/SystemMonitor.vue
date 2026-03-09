<template>
  <div class="monitor">
    <el-row :gutter="20">
      <el-col :span="8">
        <el-card>
          <div class="monitor-item">
            <div class="monitor-title">CPU 使用率</div>
            <el-progress :percentage="systemInfo.cpuUsage" :color="customColor" />
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="monitor-item">
            <div class="monitor-title">内存使用率</div>
            <el-progress :percentage="systemInfo.memoryUsagePercent" :color="customColor" />
            <div class="monitor-detail">{{ formatBytes(systemInfo.usedMemory) }} / {{ formatBytes(systemInfo.totalMemory) }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="monitor-item">
            <div class="monitor-title">磁盘使用率</div>
            <el-progress :percentage="systemInfo.diskUsagePercent" :color="customColor" />
            <div class="monitor-detail">{{ formatBytes(systemInfo.usedDisk) }} / {{ formatBytes(systemInfo.totalDisk) }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card style="margin-top: 20px">
      <template #header>
        <div class="card-header">
          <span>系统信息</span>
          <el-tag :type="health.isHealthy ? 'success' : 'danger'">{{ health.status }}</el-tag>
        </div>
      </template>
      <el-descriptions :column="2" border>
        <el-descriptions-item label="操作系统">{{ systemInfo.osVersion }}</el-descriptions-item>
        <el-descriptions-item label="处理器数量">{{ systemInfo.processorCount }}</el-descriptions-item>
        <el-descriptions-item label="运行时间">{{ formatUptime(systemInfo.uptime) }}</el-descriptions-item>
        <el-descriptions-item label="数据库状态">
          <el-tag :type="health.services['Database']?.isHealthy ? 'success' : 'danger'">
            {{ health.services['Database']?.isHealthy ? '正常' : '异常' }}
          </el-tag>
        </el-descriptions-item>
      </el-descriptions>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import api from '@/api/request';

const systemInfo = ref({ cpuUsage: 0, totalMemory: 0, usedMemory: 0, memoryUsagePercent: 0, totalDisk: 0, usedDisk: 0, diskUsagePercent: 0, osVersion: '', processorCount: 0, uptime: '' });
const health = ref({ isHealthy: true, status: 'Healthy', services: {} as any });
const customColor = (percentage: number) => {
  if (percentage < 60) return '#67c23a';
  if (percentage < 80) return '#e6a23c';
  return '#f56c6c';
};

const loadSystemInfo = async () => {
  try {
    systemInfo.value = await api.get('/monitor/system');
  } catch (error) {
    ElMessage.error('加载系统信息失败');
  }
};

const loadHealth = async () => {
  try {
    health.value = await api.get('/monitor/health');
  } catch (error) {
    console.error('加载健康状态失败', error);
  }
};

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i];
};

const formatUptime = (uptime: string) => {
  const date = new Date(uptime);
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  const hours = Math.floor(diff / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
  return `${hours}小时${minutes}分钟`;
};

onMounted(() => {
  loadSystemInfo();
  loadHealth();
  setInterval(() => {
    loadSystemInfo();
    loadHealth();
  }, 30000);
});
</script>

<style scoped>
.monitor-item {
  padding: 10px 0;
}
.monitor-title {
  font-size: 16px;
  margin-bottom: 15px;
  font-weight: bold;
}
.monitor-detail {
  margin-top: 10px;
  color: #909399;
  font-size: 14px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
