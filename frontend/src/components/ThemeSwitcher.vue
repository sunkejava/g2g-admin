<template>
  <div class="theme-switcher">
    <el-popover placement="bottom" :width="280" trigger="click">
      <template #reference>
        <el-button circle :icon="MagicStick" class="theme-btn" />
      </template>
      <div class="theme-panel">
        <h4 class="panel-title">主题颜色</h4>
        <div class="theme-colors">
          <div 
            v-for="theme in themes" 
            :key="theme.name"
            class="theme-color-item"
            :class="{ active: currentTheme === theme.name }"
            :style="{ background: theme.color }"
            @click="setTheme(theme.name)"
          >
            <el-icon v-if="currentTheme === theme.name"><Check /></el-icon>
          </div>
        </div>
        
        <h4 class="panel-title" style="margin-top: 20px">背景样式</h4>
        <div class="bg-styles">
          <el-radio-group v-model="currentBgStyle" @change="setBgStyle">
            <el-radio-button label="default">默认</el-radio-button>
            <el-radio-button label="tech">科技感</el-radio-button>
            <el-radio-button label="dark">深色</el-radio-button>
            <el-radio-button label="light">浅色</el-radio-button>
          </el-radio-group>
        </div>
      </div>
    </el-popover>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { MagicStick, Check } from '@element-plus/icons-vue';

interface Theme {
  name: string;
  color: string;
  label: string;
}

const themes: Theme[] = [
  { name: 'blue', color: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', label: '蓝色' },
  { name: 'purple', color: 'linear-gradient(135deg, #a29bfe 0%, #6c5ce7 100%)', label: '紫色' },
  { name: 'green', color: 'linear-gradient(135deg, #00b894 0%, #00cec9 100%)', label: '绿色' },
  { name: 'cyan', color: 'linear-gradient(135deg, #00cec9 0%, #0984e3 100%)', label: '青色' },
  { name: 'red', color: 'linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%)', label: '红色' },
  { name: 'orange', color: 'linear-gradient(135deg, #f0932b 0%, #eb4d4b 100%)', label: '橙色' },
  { name: 'yellow', color: 'linear-gradient(135deg, #f1c40f 0%, #f39c12 100%)', label: '黄色' },
];

const currentTheme = ref('blue');
const currentBgStyle = ref('default');

const setTheme = (themeName: string) => {
  currentTheme.value = themeName;
  document.documentElement.className = `theme-${themeName}`;
  localStorage.setItem('g2g-theme', themeName);
};

const setBgStyle = (style: string) => {
  currentBgStyle.value = style;
  document.body.className = `bg-${style}`;
  localStorage.setItem('g2g-bg-style', style);
};

onMounted(() => {
  // 从本地存储加载主题
  const savedTheme = localStorage.getItem('g2g-theme') || 'blue';
  const savedBgStyle = localStorage.getItem('g2g-bg-style') || 'default';
  
  setTheme(savedTheme);
  setBgStyle(savedBgStyle);
});
</script>

<style scoped>
.theme-switcher {
  display: inline-block;
}

.theme-btn {
  border: none;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.theme-panel {
  padding: 10px 0;
}

.panel-title {
  margin: 0 0 15px 0;
  padding: 0 15px;
  font-size: 14px;
  color: #606266;
  font-weight: 600;
}

.theme-colors {
  display: flex;
  gap: 10px;
  padding: 0 15px;
  flex-wrap: wrap;
}

.theme-color-item {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.theme-color-item:hover {
  transform: scale(1.1);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
}

.theme-color-item.active {
  box-shadow: 0 0 0 3px rgba(0, 0, 0, 0.2);
}

.bg-styles {
  padding: 0 15px;
}
</style>

<style>
/* 全局背景样式 */
body.bg-default {
  background: #f5f7fa;
}

body.bg-tech {
  background: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
  min-height: 100vh;
}

body.bg-tech::before {
  content: '';
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-image: 
    radial-gradient(circle at 20% 50%, rgba(0, 217, 255, 0.1) 0%, transparent 50%),
    radial-gradient(circle at 80% 80%, rgba(102, 126, 234, 0.1) 0%, transparent 50%);
  pointer-events: none;
  z-index: 0;
}

body.bg-dark {
  background: #1a1a2e;
}

body.bg-light {
  background: #ffffff;
}

/* 主题色变量 */
.theme-blue {
  --el-color-primary: #409eff;
  --g2g-gradient-start: #667eea;
  --g2g-gradient-end: #764ba2;
}

.theme-purple {
  --el-color-primary: #9c27b0;
  --g2g-gradient-start: #a29bfe;
  --g2g-gradient-end: #6c5ce7;
}

.theme-green {
  --el-color-primary: #67c23a;
  --g2g-gradient-start: #00b894;
  --g2g-gradient-end: #00cec9;
}

.theme-cyan {
  --el-color-primary: #00bcd4;
  --g2g-gradient-start: #00cec9;
  --g2g-gradient-end: #0984e3;
}

.theme-red {
  --el-color-primary: #f56c6c;
  --g2g-gradient-start: #ff6b6b;
  --g2g-gradient-end: #ee5a6f;
}

.theme-orange {
  --el-color-primary: #e6a23c;
  --g2g-gradient-start: #f0932b;
  --g2g-gradient-end: #eb4d4b;
}

.theme-yellow {
  --el-color-primary: #e6c200;
  --g2g-gradient-start: #f1c40f;
  --g2g-gradient-end: #f39c12;
}
</style>
