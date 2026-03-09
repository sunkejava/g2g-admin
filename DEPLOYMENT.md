# G2G 后台管理系统 - 部署指南

## 📋 目录

1. [环境准备](#环境准备)
2. [本地开发](#本地开发)
3. [生产部署](#生产部署)
4. [Docker 部署](#docker-部署)
5. [数据库迁移](#数据库迁移)
6. [常见问题](#常见问题)

## 环境准备

### 必需软件

| 软件 | 版本 | 下载地址 |
|------|------|----------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org/ |
| Git | 最新 | https://git-scm.com/ |
| Docker | 24+ | https://docker.com/ (可选) |

### 检查环境

```bash
# 检查 .NET
dotnet --version

# 检查 Node.js
node -v
npm -v

# 检查 Docker
docker --version
docker-compose --version
```

## 本地开发

### 1. 克隆项目

```bash
git clone https://github.com/sunkejava/g2g-admin.git
cd g2g-admin
```

### 2. 启动后端

```bash
cd G2G.Admin.API

# 还原依赖
dotnet restore

# 运行开发服务器
dotnet run --urls="http://0.0.0.0:5000"
```

首次运行会自动创建数据库和种子数据。

### 3. 启动前端

```bash
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

访问 http://localhost:5173

### 4. 默认登录

```
用户名：admin
密码：admin123
```

## 生产部署

### 方式一：直接部署

#### 1. 后端发布

```bash
cd G2G.Admin.API

# 发布 Release 版本
dotnet publish -c Release -o ./publish

# 运行
cd publish
dotnet G2G.Admin.API.dll --urls="http://0.0.0.0:5000"
```

#### 2. 前端构建

```bash
cd frontend

# 安装依赖
npm install

# 构建生产版本
npm run build

# 预览
npm run preview -- --host 0.0.0.0 --port 80
```

#### 3. 配置 Nginx (可选)

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        root /path/to/g2g-admin/frontend/dist;
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 方式二：Systemd 服务 (Linux)

#### 1. 创建服务文件

```bash
sudo nano /etc/systemd/system/g2g-backend.service
```

内容：
```ini
[Unit]
Description=G2G Backend Service
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/path/to/g2g-admin/G2G.Admin.API/publish
ExecStart=/usr/bin/dotnet /path/to/g2g-admin/G2G.Admin.API/publish/G2G.Admin.API.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

#### 2. 启动服务

```bash
# 重载 systemd
sudo systemctl daemon-reload

# 启用服务
sudo systemctl enable g2g-backend

# 启动服务
sudo systemctl start g2g-backend

# 查看状态
sudo systemctl status g2g-backend
```

## Docker 部署

### 方式一：Docker Compose (推荐)

#### 1. 启动服务

```bash
cd g2g-admin

# 构建并启动
docker-compose up -d --build

# 查看日志
docker-compose logs -f

# 停止服务
docker-compose down
```

#### 2. 访问地址

- 前端：http://localhost
- 后端：http://localhost:5000
- Swagger：http://localhost:5000/swagger

### 方式二：单独 Docker 容器

#### 1. 构建镜像

```bash
# 后端镜像
cd G2G.Admin.API
docker build -t g2g-backend:latest .

# 前端镜像
cd ../frontend
docker build -t g2g-frontend:latest .
```

#### 2. 运行容器

```bash
# 启动后端
docker run -d \
  --name g2g-backend \
  -p 5000:8080 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/logs:/app/logs \
  g2g-backend:latest

# 启动前端
docker run -d \
  --name g2g-frontend \
  -p 80:80 \
  --link g2g-backend:backend \
  g2g-frontend:latest
```

## 数据库迁移

### 添加新迁移

```bash
cd G2G.Admin.API

# 添加迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update

# 删除迁移 (如果需要)
dotnet ef migrations remove
```

### 切换数据库

#### PostgreSQL

1. 安装 NuGet 包
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

2. 修改连接字符串
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=g2g_admin;Username=postgres;Password=your_password"
}
```

3. 修改 Program.cs
```csharp
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
```

#### MySQL

1. 安装 NuGet 包
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

2. 修改连接字符串
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=g2g_admin;User=root;Password=your_password"
}
```

## 常见问题

### 1. 端口被占用

**错误**: `Failed to bind to address http://0.0.0.0:5000`

**解决**:
```bash
# 查找占用端口的进程
lsof -i :5000

# 杀死进程
kill -9 <PID>

# 或修改端口
dotnet run --urls="http://0.0.0.0:5001"
```

### 2. 数据库锁定

**错误**: `database is locked`

**解决**:
```bash
# 删除数据库文件 (会丢失数据！)
rm G2G.Admin.API/g2g-admin.db

# 重启应用
dotnet run
```

### 3. 前端依赖安装失败

**错误**: `npm install` 卡住或失败

**解决**:
```bash
# 清理缓存
npm cache clean --force

# 使用淘宝镜像
npm config set registry https://registry.npmmirror.com

# 重新安装
rm -rf node_modules package-lock.json
npm install
```

### 4. Docker 容器无法启动

**错误**: 容器启动后立即退出

**解决**:
```bash
# 查看日志
docker logs <container-name>

# 检查端口占用
docker ps -a

# 重新构建
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### 5. JWT Token 失效

**症状**: 登录后很快被踢出

**解决**: 修改 `appsettings.json`
```json
"JwtSettings": {
  "ExpirationMinutes": 120  // 延长过期时间
}
```

## 性能优化

### 1. 启用响应压缩

在 `Program.cs` 中添加:
```csharp
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

### 2. 配置缓存

```csharp
// 添加内存缓存
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
```

### 3. 数据库优化

```csharp
// 启用查询缓存
options.UseSqlite(connectionString, b => 
    b.MigrationsAssembly(typeof(G2GDbContext).Assembly.FullName));
```

## 监控与日志

### 查看日志

```bash
# 后端日志
tail -f G2G.Admin.API/logs/g2g-admin-*.log

# Docker 日志
docker-compose logs -f backend
```

### 健康检查

```bash
curl http://localhost:5000/api/monitor/health
```

## 备份与恢复

### 备份数据库

```bash
# SQLite
cp G2G.Admin.API/g2g-admin.db g2g-admin-backup-$(date +%Y%m%d).db

# PostgreSQL
pg_dump -U postgres g2g_admin > backup.sql

# MySQL
mysqldump -u root -p g2g_admin > backup.sql
```

### 恢复数据库

```bash
# SQLite
cp backup.db G2G.Admin.API/g2g-admin.db

# PostgreSQL
psql -U postgres g2g_admin < backup.sql

# MySQL
mysql -u root -p g2g_admin < backup.sql
```

---

**部署完成！如有问题请查看 GitHub Issues。**
