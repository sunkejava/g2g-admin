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
            <div class="stat-title">
              日志总数
              <el-tooltip content="所有时间的日志总数，日志管理页面默认显示近 7 天数据" placement="bottom">
                <el-icon style="margin-left: 5px; cursor: help"><QuestionFilled /></el-icon>
              </el-tooltip>
            </div>
            <div class="stat-value">{{ stats.totalOperationLogs + stats.totalLoginLogs + stats.totalSystemLogs }}</div>
            <div class="stat-detail">
              操作：{{ stats.totalOperationLogs }} | 系统：{{ stats.totalSystemLogs }} | 登录：{{ stats.totalLoginLogs }}
            </div>
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
import { QuestionFilled } from '@element-plus/icons-vue';
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
  padding: 30px 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9ff 100%);
  border-radius: 16px;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(102, 126, 234, 0.1) 0%, transparent 70%);
  animation: pulse 3s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 0.5;
  }
  50% {
    transform: scale(1.1);
    opacity: 0.8;
  }
}

.stat-title {
  color: #606266;
  font-size: 14px;
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 1;
}

.stat-value {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  font-size: 42px;
  font-weight: 800;
  position: relative;
  z-index: 1;
  animation: numberFlip 0.5s ease-out;
}

@keyframes numberFlip {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.stat-detail {
  color: #909399;
  font-size: 12px;
  margin-top: 12px;
  position: relative;
  z-index: 1;
  background: rgba(255, 255, 255, 0.8);
  padding: 4px 12px;
  border-radius: 12px;
  display: inline-block;
}

.dashboard {
  animation: fadeIn 0.5s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
