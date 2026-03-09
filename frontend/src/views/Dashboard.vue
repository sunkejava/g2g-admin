<template>
  <div class="dashboard">
    <el-row :gutter="20">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-title">用户总数</div>
            <div class="stat-value">{{ stats.totalUsers }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-title">角色总数</div>
            <div class="stat-value">{{ stats.totalRoles }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-title">版本总数</div>
            <div class="stat-value">{{ stats.totalVersions }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-title">日志总数</div>
            <div class="stat-value">{{ stats.totalOperationLogs + stats.totalLoginLogs + stats.totalSystemLogs }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="12">
        <el-card>
          <template #header>用户增长趋势</template>
          <div ref="userTrendChart" style="height: 300px"></div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>日志分布</template>
          <div ref="logDistChart" style="height: 300px"></div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import * as echarts from 'echarts';
import api from '@/api/request';

const stats = ref({ totalUsers: 0, totalRoles: 0, totalVersions: 0, totalOperationLogs: 0, totalLoginLogs: 0, totalSystemLogs: 0 });
const userTrendChart = ref();
const logDistChart = ref();

const loadStats = async () => {
  try {
    stats.value = await api.get('/dashboard/stats');
  } catch (error) {
    console.error('加载统计数据失败', error);
  }
};

const initCharts = async () => {
  try {
    const trend = await api.get('/dashboard/user-trend');
    const logDist = await api.get('/dashboard/log-distribution');

    // 用户趋势图
    const userChart = echarts.init(userTrendChart.value!);
    userChart.setOption({
      xAxis: { type: 'category', data: trend.dates },
      yAxis: { type: 'value' },
      series: [{ data: trend.counts, type: 'line', smooth: true, itemStyle: { color: '#409EFF' } }],
    });

    // 日志分布图
    const logChart = echarts.init(logDistChart.value!);
    logChart.setOption({
      tooltip: { trigger: 'item' },
      series: [{
        type: 'pie',
        radius: '50%',
        data: [
          { value: logDist.operationLogs, name: '操作日志' },
          { value: logDist.systemLogs, name: '系统日志' },
          { value: logDist.loginLogs, name: '登录日志' },
        ],
      }],
    });

    window.addEventListener('resize', () => {
      userChart.resize();
      logChart.resize();
    });
  } catch (error) {
    console.error('加载图表数据失败', error);
  }
};

onMounted(() => {
  loadStats();
  initCharts();
});
</script>

<style scoped>
.stat-card {
  text-align: center;
  padding: 20px 0;
}

.stat-title {
  color: #909399;
  font-size: 14px;
  margin-bottom: 10px;
}

.stat-value {
  color: #303133;
  font-size: 32px;
  font-weight: bold;
}
</style>
