import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    port: 5173,
    host: '0.0.0.0', // 允许局域网访问
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  },
  build: {
    // 生产环境输出到后端 wwwroot 目录
    outDir: path.resolve(__dirname, '../G2G.Admin.API/wwwroot'),
    emptyOutDir: true, // 自动清空目标目录
    assetsDir: 'assets',
    sourcemap: false
  }
})
