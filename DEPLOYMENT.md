# 🚀 G2G Admin 部署指南

## 快速部署

### 本地开发部署

#### 方式一：一键脚本（推荐）
```bash
# 部署（首次运行）
./deploy.sh  # Linux/Mac
deploy.bat   # Windows

# 启动
./start.sh   # Linux/Mac
start.bat    # Windows
```

#### 方式二：手动命令
```bash
# 1. 构建前端到后端 wwwroot
cd frontend
npm install
npm run build
# 前端自动输出到 ../G2G.Admin.API/wwwroot

# 2. 启动后端
cd ../G2G.Admin.API
dotnet run --urls="http://localhost:5000"

# 3. 访问
# http://localhost:5000
```

---

## CI/CD 流程

### GitHub Actions

#### CI 流程（ci.yml）
```yaml
触发条件:
  - push 到 main/develop 分支
  - pull request 到 main 分支

执行步骤:
  1. 设置 .NET 10 和 Node.js 20
  2. 安装前端依赖
  3. 构建前端到 G2G.Admin.API/wwwroot
  4. 还原后端依赖
  5. 构建后端（包含前端）
  6. 运行后端测试
  7. 发布到 publish 目录
  8. 验证 wwwroot 存在
  9. 上传构建产物
```

#### CD 流程（cd.yml）
```yaml
触发条件:
  - push 到 main 分支

执行步骤:
  1. 设置 .NET 10 和 Node.js 20
  2. 安装前端依赖
  3. 构建前端到 G2G.Admin.API/wwwroot
  4. 验证 wwwroot 目录
  5. 构建 Docker 镜像（包含前后端）
  6. 推送到 Docker Hub
```

### Docker 构建流程

```dockerfile
阶段 1: 前端构建
  FROM node:20-alpine
  → npm ci
  → npm run build
  → 输出到 /frontend/dist

阶段 2: 后端构建
  FROM mcr.microsoft.com/dotnet/sdk:10.0
  → dotnet restore
  → 复制前端构建结果到 wwwroot
  → dotnet publish

阶段 3: 运行环境
  FROM mcr.microsoft.com/dotnet/aspnet:10.0
  → 复制发布文件（包含 wwwroot）
  → 启动应用
```

---

## Docker 部署

### 方式一：docker-compose（推荐）

```bash
# 构建并启动
docker-compose up -d

# 查看日志
docker-compose logs -f

# 停止
docker-compose down
```

**访问**: http://localhost:5000

### 方式二：手动 Docker

```bash
# 构建镜像
docker build -t g2g-admin ./G2G.Admin.API

# 启动容器
docker run -d \
  -p 5000:8080 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/uploads:/app/uploads \
  -v $(pwd)/logs:/app/logs \
  --name g2g-admin \
  g2g-admin

# 查看日志
docker logs -f g2g-admin
```

---

## 生产环境部署

### 1. 服务器要求

- **系统**: Linux (Ubuntu 20.04+) / Windows Server 2019+
- **CPU**: 2 核+
- **内存**: 4GB+
- **磁盘**: 10GB+

### 2. 安装依赖

#### Linux (Ubuntu)
```bash
# 安装 .NET 10 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# 安装 Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# 安装 Docker（可选）
curl -fsSL https://get.docker.com | sh
```

#### Windows
```powershell
# 使用 winget 安装
winget install Microsoft.DotNet.SDK.10
winget install OpenJS.NodeJS.LTS
```

### 3. 部署步骤

#### 方式 A: 直接部署
```bash
# 1. 克隆代码
git clone https://github.com/sunkejava/g2g-admin.git
cd g2g-admin

# 2. 执行部署脚本
./deploy.sh

# 3. 启动服务
./start.sh

# 4. 配置 systemd 服务（Linux）
sudo cp g2g-admin.service /etc/systemd/system/
sudo systemctl enable g2g-admin
sudo systemctl start g2g-admin
```

#### 方式 B: Docker 部署
```bash
# 1. 拉取镜像
docker pull sunkejava/g2g-admin:latest

# 2. 启动容器
docker run -d \
  -p 80:8080 \
  -v /opt/g2g-admin/data:/app/data \
  -v /opt/g2g-admin/uploads:/app/uploads \
  -v /opt/g2g-admin/logs:/app/logs \
  --restart unless-stopped \
  --name g2g-admin \
  sunkejava/g2g-admin:latest
```

### 4. Nginx 反向代理（可选）

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 配置说明

### 环境变量

| 变量 | 说明 | 默认值 |
|------|------|--------|
| ASPNETCORE_ENVIRONMENT | 运行环境 | Production |
| ASPNETCORE_URLS | 监听地址 | http://+:8080 |
| ConnectionStrings__DefaultConnection | 数据库连接 | Data Source=/app/data/g2g-admin.db |
| JwtSettings__SecretKey | JWT 密钥 | （必须配置） |
| JwtSettings__Issuer | JWT 颁发者 | G2GAdmin |
| JwtSettings__Audience | JWT 受众 | G2GAdminUser |
| JwtSettings__ExpirationMinutes | Token 过期时间 | 60 |

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/opt/g2g-admin/data/g2g-admin.db"
  },
  "JwtSettings": {
    "SecretKey": "你的超长密钥（至少 32 位字符）",
    "Issuer": "G2GAdmin",
    "Audience": "G2GAdminUser",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Warning"
  }
}
```

---

## 健康检查

```bash
# API 健康检查
curl http://localhost:5000/api/monitor/health

# Swagger UI
curl http://localhost:5000/swagger

# 前端页面
curl http://localhost:5000/
```

---

## 日志管理

### 日志位置
- **Docker**: `/app/logs`
- **直接部署**: `G2G.Admin.API/logs/`

### 日志级别
- Information: 一般信息
- Warning: 警告
- Error: 错误
- Critical: 严重错误

### 日志清理
```bash
# 删除 30 天前的日志
find logs/ -name "*.log" -mtime +30 -delete
```

---

## 备份与恢复

### 数据库备份
```bash
# 备份
cp data/g2g-admin.db backup/g2g-admin-$(date +%Y%m%d).db

# 恢复
cp backup/g2g-admin-20260310.db data/g2g-admin.db
```

### 完整备份
```bash
tar -czf g2g-admin-backup-$(date +%Y%m%d).tar.gz \
  data/ \
  uploads/ \
  appsettings.Production.json
```

---

## 故障排查

### 1. 前端页面 404
```bash
# 检查 wwwroot 是否存在
ls -la G2G.Admin.API/wwwroot

# 重新构建前端
cd frontend && npm run build
```

### 2. 数据库锁定
```bash
# 删除锁文件
rm data/g2g-admin.db-shm data/g2g-admin.db-wal

# 重启服务
```

### 3. 端口占用
```bash
# 查看占用端口的进程
lsof -i :5000

# 杀死进程
kill -9 <PID>
```

---

**最后更新**: 2026-03-10
