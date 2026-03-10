# 🪟 Windows 部署指南

## 🚀 快速开始（推荐）

### 一键部署和启动

#### 步骤 1: 环境准备
```cmd
# 安装 .NET 10 SDK
https://dotnet.microsoft.com/download

# 安装 Node.js LTS
https://nodejs.org/
```

#### 步骤 2: 获取代码
```cmd
cd C:\Projects
git clone https://github.com/sunkejava/g2g-admin.git
cd g2g-admin
```

#### 步骤 3: 一键部署
双击运行 `deploy.bat`

或命令行：
```cmd
deploy.bat
```

#### 步骤 4: 启动服务
双击运行 `start.bat`

或命令行：
```cmd
start.bat
```

#### 步骤 5: 访问系统
打开浏览器：http://localhost:5000

**默认账号**:
- 用户名：`admin`
- 密码：`admin123`

---

## 详细说明

### 架构说明

**新部署方式**:
- ✅ 前端打包后直接输出到后端 `wwwroot` 目录
- ✅ 只需启动后端服务，即可访问前后端完整功能
- ✅ 无需单独启动前端开发服务器
- ✅ 生产环境部署更简单

**文件结构**:
```
g2g-admin/
├── G2G.Admin.API/
│   ├── wwwroot/          # 前端打包文件（自动输出）
│   ├── G2G.Admin.API.dll # 后端程序
│   └── g2g-admin.db      # SQLite 数据库
├── frontend/             # 前端源码
├── deploy.bat            # 一键部署脚本
└── start.bat             # 一键启动脚本
```

---

### 开发环境运行

#### 方式一：使用脚本（推荐）
```cmd
:: 部署（首次运行）
deploy.bat

:: 启动
start.bat
```

#### 方式二：手动命令

**1. 构建前端并输出到后端**
```cmd
cd frontend
npm install
npm run build
:: 文件自动输出到 ../G2G.Admin.API/wwwroot
```

**2. 启动后端**
```cmd
cd G2G.Admin.API
dotnet run --urls="http://localhost:5000"
```

**3. 访问**
- 前端：http://localhost:5000
- Swagger: http://localhost:5000/swagger

---

### 生产环境部署

#### 步骤 1: 发布
```cmd
:: 执行一键部署
deploy.bat
```

#### 步骤 2: 配置（可选）
编辑 `publish/appsettings.Production.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "你的超长密钥（至少 32 位）"
  },
  "Serilog": {
    "MinimumLevel": "Warning"
  }
}
```

#### 步骤 3: 启动
```cmd
cd publish
dotnet G2G.Admin.API.dll --urls="http://0.0.0.0:5000"
```

#### 步骤 4: 创建 Windows 服务（可选）

使用 NSSM:
```cmd
nssm install G2GAdmin
nssm set G2GAdmin Application "C:\Program Files\dotnet\dotnet.exe"
nssm set G2GAdmin AppParameters "C:\Projects\g2g-admin\publish\G2G.Admin.API.dll"
nssm set G2GAdmin AppDirectory "C:\Projects\g2g-admin\publish"
nssm start G2GAdmin
```

---

### 5. 登录系统

**默认管理员账号**:
- 用户名：`admin`
- 密码：`admin123`

---

## 生产环境部署

### 1. 后端发布

#### 步骤 1: 发布后端
```cmd
cd C:\Projects\g2g-admin\G2G.Admin.API
dotnet publish -c Release -o publish
```

#### 步骤 2: 配置生产环境
编辑 `publish/appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=g2g-admin.db"
  },
  "JwtSettings": {
    "SecretKey": "你的超长密钥（至少 32 位）",
    "Issuer": "G2GAdmin",
    "Audience": "G2GAdminUser",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Warning"
  }
}
```

#### 步骤 3: 复制数据库
```cmd
copy G2G.Admin.API\g2g-admin.db publish\
```

#### 步骤 4: 创建 Windows 服务（可选）

使用 NSSM (Non-Sucking Service Manager):

1. 下载 [NSSM](https://nssm.cc/download)
2. 解压到 `C:\Program Files\nssm`
3. 以管理员身份运行命令提示符：
   ```cmd
   cd C:\Program Files\nssm
   nssm install G2GAdmin
   ```
4. 配置服务：
   - Path: `C:\Program Files\dotnet\dotnet.exe`
   - Arguments: `C:\Projects\g2g-admin\G2G.Admin.API\publish\G2G.Admin.API.dll`
   - Startup directory: `C:\Projects\g2g-admin\G2G.Admin.API\publish`
5. 启动服务：
   ```cmd
   nssm start G2GAdmin
   ```

---

### 2. 前端发布

#### 步骤 1: 构建生产版本
```cmd
cd C:\Projects\g2g-admin\frontend
npm run build
```

#### 步骤 2: 配置 Nginx（推荐）

1. 下载 [Nginx for Windows](http://nginx.org/en/download.html)
2. 解压到 `C:\nginx`
3. 编辑 `conf/nginx.conf`:
   ```nginx
   server {
       listen 80;
       server_name localhost;
       
       location / {
           root C:/Projects/g2g-admin/frontend/dist;
           try_files $uri $uri/ /index.html;
       }
       
       location /api/ {
           proxy_pass http://localhost:5000/;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
       }
   }
   ```
4. 启动 Nginx:
   ```cmd
   cd C:\nginx
   start nginx
   ```

#### 步骤 3: 访问
打开浏览器访问：http://localhost

---

## Docker 部署（推荐）

### 前提条件
- 安装 [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)

### 步骤 1: 构建镜像
```cmd
cd C:\Projects\g2g-admin
docker-compose build
```

### 步骤 2: 启动容器
```cmd
docker-compose up -d
```

### 步骤 3: 访问
- 前端：http://localhost
- 后端 API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### 步骤 4: 查看日志
```cmd
docker-compose logs -f
```

### 步骤 5: 停止服务
```cmd
docker-compose down
```

---

## 常见问题排查

### 1. 端口被占用

**错误**: `Failed to bind to address http://localhost:5000`

**解决**:
```cmd
# 查找占用端口的进程
netstat -ano | findstr :5000

# 杀死进程（替换 PID）
taskkill /F /PID <PID>

# 或修改端口
dotnet run --urls="http://localhost:5001"
```

### 2. 前端无法连接后端

**错误**: `Network Error` 或 `CORS error`

**检查**:
1. 后端是否启动：http://localhost:5000/swagger
2. 前端代理配置 `frontend/vite.config.ts`:
   ```typescript
   proxy: {
     '/api': {
       target: 'http://localhost:5000',
       changeOrigin: true
     }
   }
   ```

### 3. 数据库锁定

**错误**: `database is locked`

**解决**:
1. 关闭所有后端进程
2. 删除数据库锁文件：
   ```cmd
   del G2G.Admin.API\g2g-admin.db-shm
   del G2G.Admin.API\g2g-admin.db-wal
   ```
3. 重启后端

### 4. npm install 失败

**错误**: `npm ERR! network timeout`

**解决**:
```cmd
# 切换淘宝镜像
npm config set registry https://registry.npmmirror.com

# 清理缓存
npm cache clean --force

# 重新安装
npm install
```

### 5. 登录失败

**问题**: 密码错误或 Token 过期

**解决**:
1. 检查数据库是否有 admin 用户：
   ```cmd
   # 使用 DB Browser for SQLite 打开 g2g-admin.db
   # 执行：SELECT * FROM Users WHERE Username = 'admin';
   ```
2. 重置密码（使用 SQLite 工具）:
   ```sql
   -- 需要生成新的 BCrypt 哈希
   -- 推荐使用在线工具生成：https://bcrypt-generator.com/
   UPDATE Users SET PasswordHash = '$2a$11$...' WHERE Username = 'admin';
   ```

### 6. 防火墙阻止访问

**问题**: 局域网无法访问

**解决**:
1. 打开 Windows 防火墙设置
2. 添加入站规则：
   - 端口：5000 (后端), 5173 (前端)
   - 协议：TCP
   - 操作：允许连接

或使用命令行：
```cmd
netsh advfirewall firewall add rule name="G2G Backend" dir=in action=allow protocol=TCP localport=5000
netsh advfirewall firewall add rule name="G2G Frontend" dir=in action=allow protocol=TCP localport=5173
```

---

## 性能优化

### 1. 启用 HTTPS（生产环境）

#### 使用 IIS
1. 安装 IIS 和 URL Rewrite 模块
2. 创建网站，指向 `frontend/dist`
3. 配置反向代理到后端
4. 绑定 SSL 证书

#### 使用 Kestrel + HTTPS
```cmd
# 创建自签名证书
dotnet dev-certs https --trust

# 配置 appsettings.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

### 2. 数据库优化

#### 定期清理日志
```sql
-- 删除 30 天前的日志
DELETE FROM OperationLogs WHERE CreatedAt < datetime('now', '-30 days');
DELETE FROM SystemLogs WHERE CreatedAt < datetime('now', '-30 days');
DELETE FROM LoginLogs WHERE CreatedAt < datetime('now', '-30 days');

-- 压缩数据库
VACUUM;
```

### 3. 前端优化

#### 启用 Gzip 压缩（Nginx）
```nginx
gzip on;
gzip_types text/plain text/css application/json application/javascript;
gzip_min_length 1000;
```

---

## 自动化脚本

### 一键启动脚本 (start.bat)
```batch
@echo off
echo Starting G2G Admin...

:: 启动后端
start "G2G Backend" cmd /k "cd G2G.Admin.API && dotnet run --urls=http://localhost:5000"

:: 等待 3 秒
timeout /t 3 /nobreak

:: 启动前端
start "G2G Frontend" cmd /k "cd frontend && npm run dev"

echo Services started!
echo Backend: http://localhost:5000
echo Frontend: http://localhost:5173
pause
```

### 一键构建脚本 (build.bat)
```batch
@echo off
echo Building G2G Admin...

:: 构建后端
echo Building backend...
cd G2G.Admin.API
dotnet publish -c Release -o publish
cd ..

:: 构建前端
echo Building frontend...
cd frontend
call npm run build
cd ..

echo Build completed!
pause
```

---

## 系统要求

### 最低配置
- **CPU**: 双核 2.0 GHz
- **内存**: 4 GB RAM
- **磁盘**: 1 GB 可用空间
- **系统**: Windows 10 或更高版本

### 推荐配置
- **CPU**: 四核 2.5 GHz+
- **内存**: 8 GB RAM
- **磁盘**: SSD，5 GB 可用空间
- **系统**: Windows 11

---

## 技术支持

- **GitHub Issues**: https://github.com/sunkejava/g2g-admin/issues
- **文档**: https://github.com/sunkejava/g2g-admin/blob/main/README.md

---

**最后更新**: 2026-03-10
