<template>
  <div class="system-settings">
    <el-row :gutter="20">
      <!-- 系统基本信息 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>🏷️ 系统基本信息</span>
              <el-button type="primary" size="small" @click="handleSaveBasic">保存配置</el-button>
            </div>
          </template>
          <el-form :model="basicConfig" label-width="100px" size="default">
            <el-form-item label="系统名称">
              <el-input v-model="basicConfig.systemName" placeholder="如：G2G 后台管理系统" />
            </el-form-item>
            <el-form-item label="系统图标">
              <el-input v-model="basicConfig.systemIcon" placeholder="如：/g2g-logo.svg" />
              <div v-if="basicConfig.systemIcon" style="margin-top: 10px">
                <img :src="basicConfig.systemIcon" alt="System Icon" style="height: 64px; border-radius: 8px" />
              </div>
            </el-form-item>
            <el-form-item label="版本号">
              <el-input v-model="basicConfig.version" placeholder="如：1.0.0" />
            </el-form-item>
            <el-form-item label="版权信息">
              <el-input v-model="basicConfig.copyright" placeholder="如：© 2026 G2G Team" />
            </el-form-item>
            <el-form-item label="技术支持">
              <el-input v-model="basicConfig.support" placeholder="如：support@g2g.com" />
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- 系统运行时信息 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>💻 系统运行时信息</span>
            </div>
          </template>
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item label="系统名称">{{ systemInfo.name }}</el-descriptions-item>
            <el-descriptions-item label="版本号">{{ systemInfo.version }}</el-descriptions-item>
            <el-descriptions-item label="运行环境">{{ systemInfo.environment }}</el-descriptions-item>
            <el-descriptions-item label="启动时间">{{ formatUptime(systemInfo.uptime) }}</el-descriptions-item>
            <el-descriptions-item label="操作系统">{{ systemInfo.osVersion }}</el-descriptions-item>
            <el-descriptions-item label=".NET 版本">{{ systemInfo.dotnetVersion }}</el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card style="margin-top: 20px">
          <template #header>
            <div class="card-header">
              <span>📊 系统资源</span>
            </div>
          </template>
          <div class="monitor-item">
            <div class="monitor-title">CPU 使用率</div>
            <el-progress :percentage="systemInfo.cpuUsage" :color="getColor(systemInfo.cpuUsage)" />
          </div>
          <div class="monitor-item" style="margin-top: 15px">
            <div class="monitor-title">内存使用率</div>
            <el-progress :percentage="systemInfo.memoryUsagePercent" :color="getColor(systemInfo.memoryUsagePercent)" />
            <div class="monitor-detail">{{ formatBytes(systemInfo.usedMemory) }} / {{ formatBytes(systemInfo.totalMemory) }}</div>
          </div>
          <div class="monitor-item" style="margin-top: 15px">
            <div class="monitor-title">磁盘使用率</div>
            <el-progress :percentage="systemInfo.diskUsagePercent" :color="getColor(systemInfo.diskUsagePercent)" />
            <div class="monitor-detail">{{ formatBytes(systemInfo.usedDisk) }} / {{ formatBytes(systemInfo.totalDisk) }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 高级配置 -->
    <el-card style="margin-top: 20px">
      <template #header>
        <div class="card-header">
          <span>⚙️ 高级配置</span>
          <el-button type="warning" size="small" @click="handleClearCache">清除缓存</el-button>
        </div>
      </template>
      <el-table :data="advancedSettings" style="width: 100%" v-loading="loading">
        <el-table-column prop="key" label="配置键" width="200" />
        <el-table-column prop="value" label="配置值" show-overflow-tooltip />
        <el-table-column prop="description" label="描述" show-overflow-tooltip />
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEditAdvanced(row)">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 编辑高级配置对话框 -->
    <el-dialog v-model="dialogVisible" title="编辑配置" width="500px">
      <el-form :model="editForm" label-width="100px">
        <el-form-item label="配置键">
          <el-input v-model="editForm.key" disabled />
        </el-form-item>
        <el-form-item label="配置值">
          <el-input v-model="editForm.value" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="editForm.description" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveAdvanced">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import api from '@/api/request';

const loading = ref(false);
const dialogVisible = ref(false);

// 基本配置
const basicConfig = reactive({
  systemName: 'G2G 后台管理系统',
  systemIcon: '/g2g-logo.svg',
  version: '1.1.0',
  copyright: '© 2026 G2G Team',
  support: 'support@g2g.com'
});

// 系统信息
const systemInfo = ref({
  name: 'G2G Admin',
  version: '1.1.0',
  environment: 'Production',
  uptime: new Date(),
  osVersion: '',
  dotnetVersion: '',
  cpuUsage: 0,
  totalMemory: 0,
  usedMemory: 0,
  memoryUsagePercent: 0,
  totalDisk: 0,
  usedDisk: 0,
  diskUsagePercent: 0
});

// 高级配置
const advancedSettings = ref<any[]>([]);
const editForm = reactive({ key: '', value: '', description: '' });

const getColor = (percentage: number) => {
  if (percentage < 60) return '#67c23a';
  if (percentage < 80) return '#e6a23c';
  return '#f56c6c';
};

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i];
};

const formatUptime = (uptime: string | Date) => {
  const date = new Date(uptime);
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
  return `${days}天${hours}小时${minutes}分钟`;
};

const loadBasicConfig = async () => {
  try {
    const settings = await api.get('/settings');
    const basicSetting = settings.find((s: any) => s.key === 'System.Basic');
    if (basicSetting?.value) {
      const config = JSON.parse(basicSetting.value);
      if (config.systemName) basicConfig.systemName = config.systemName;
      if (config.systemIcon) basicConfig.systemIcon = config.systemIcon;
      if (config.version) basicConfig.version = config.version;
      if (config.copyright) basicConfig.copyright = config.copyright;
      if (config.support) basicConfig.support = config.support;
    }
  } catch (error) {
    console.error('加载基本配置失败', error);
  }
};

const handleSaveBasic = async () => {
  try {
    await api.put('/settings/System.Basic', {
      value: JSON.stringify({
        systemName: basicConfig.systemName,
        systemIcon: basicConfig.systemIcon,
        version: basicConfig.version,
        copyright: basicConfig.copyright,
        support: basicConfig.support
      }),
      description: '系统基本配置（JSON 格式）'
    });
    ElMessage.success('保存成功');
    // 更新页面标题
    document.title = basicConfig.systemName;
  } catch (error) {
    ElMessage.error('保存失败');
  }
};

const loadSystemInfo = async () => {
  try {
    const [monitorData, healthData] = await Promise.all([
      api.get('/monitor/system'),
      api.get('/monitor/health')
    ]);

    systemInfo.value = {
      ...systemInfo.value,
      ...monitorData,
      name: basicConfig.systemName,
      version: basicConfig.version
    };
  } catch (error) {
    console.error('加载系统信息失败', error);
  }
};

const loadAdvancedSettings = async () => {
  loading.value = true;
  try {
    const settings = await api.get('/settings');
    advancedSettings.value = settings.filter((s: any) => s.key !== 'System.Basic');
  } catch (error) {
    ElMessage.error('加载配置失败');
  } finally {
    loading.value = false;
  }
};

const handleEditAdvanced = (row: any) => {
  Object.assign(editForm, { key: row.key, value: row.value, description: row.description });
  dialogVisible.value = true;
};

const handleSaveAdvanced = async () => {
  try {
    await api.put(`/settings/${editForm.key}`, {
      value: editForm.value,
      description: editForm.description
    });
    ElMessage.success('保存成功');
    dialogVisible.value = false;
    loadAdvancedSettings();
  } catch (error) {
    ElMessage.error('保存失败');
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
  loadBasicConfig();
  loadSystemInfo();
  loadAdvancedSettings();
  
  // 每 30 秒刷新系统资源信息
  setInterval(() => {
    loadSystemInfo();
  }, 30000);
});
</script>

<style scoped>
.system-settings {
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

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.monitor-item {
  padding: 5px 0;
}

.monitor-title {
  font-size: 14px;
  margin-bottom: 10px;
  font-weight: bold;
  color: #606266;
}

.monitor-detail {
  margin-top: 8px;
  color: #909399;
  font-size: 12px;
}
</style>
