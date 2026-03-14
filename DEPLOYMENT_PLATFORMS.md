# G2G Admin 跨平台部署指南

## ✅ 跨平台支持

G2G Admin **完全支持跨平台运行**，基于 .NET 10 的跨平台特性。

### 支持的平台

| 操作系统 | 支持 | 说明 |
|----------|------|------|
| **Windows** | ✅ | Windows 10/11, Windows Server 2016+ |
| **Linux** | ✅ | Ubuntu, Debian, CentOS, Alpine 等 |
| **macOS** | ✅ | macOS 10.15+ (Intel/Apple Silicon) |
| **Docker** | ✅ | 任何支持 Docker 的平台 |

---

## 🚀 部署方式

### 方式一：Docker 部署（推荐）⭐

**无需安装 .NET 运行时**，Docker 镜像已包含所有依赖。

```bash
# 克隆项目
git clone https://github.com/sunkejava/g2g-admin.git
cd g2g-admin

# 一键启动
docker-compose up -d

# 查看日志
docker-compose logs -f

# 停止服务
docker-compose down
```

**访问地址：**
- 前端：http://localhost:5000
- 后端 API：http://localhost:5000/api
- Swagger：http://localhost:5000/swagger

---

### 方式二：直接运行（需要 .NET 运行时）

#### 1. 安装 .NET 10 运行时

**Windows:**
```powershell
# 下载安装程序
# https://dotnet.microsoft.com/download/dotnet/10.0
# 运行 dotnet-sdk-10.0.x-win-x64.exe
```

**Ubuntu/Debian:**
```bash
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

**CentOS/RHEL:**
```bash
sudo yum install -y https://packages.microsoft.com/config/centos/8/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-10.0
```

**macOS:**
```bash
brew install --cask dotnet-sdk
```

#### 2. 验证安装
```bash
dotnet --version
# 输出：10.0.x
```

#### 3. 运行应用
```bash
# 克隆项目
git clone https://github.com/sunkejava/g2g-admin.git
cd g2g-admin/G2G.Admin.API

# 还原依赖
dotnet restore

# 运行
dotnet run --urls="http://0.0.0.0:5000"
```

---

### 方式三：独立发布（无需安装运行时）

发布为**独立应用**，包含 .NET 运行时，目标机器无需安装任何依赖。

```bash
cd G2G.Admin.API

# Windows x64
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/win-x64

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish/linux-x64

# macOS x64
dotnet publish -c Release -r osx-x64 --self-contained -o ./publish/osx-x64

# macOS ARM (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained -o ./publish/osx-arm64
```

**发布后的文件结构：**
```
publish/
├── G2G.Admin.API.exe (Windows) 或 G2G.Admin.API (Linux/macOS)
├── 所有依赖的 DLL 文件
└── 包含 .NET 运行时（约 70MB）
```

**运行：**
```bash
# Windows
.\publish\win-x64\G2G.Admin.API.exe

# Linux/macOS
./publish/linux-x64/G2G.Admin.API
```

---

## 📊 部署方式对比

| 方式 | 优点 | 缺点 | 推荐场景 |
|------|------|------|----------|
| **Docker** | 环境隔离、一键部署、跨平台 | 需要 Docker | 生产环境、CI/CD |
| **直接运行** | 简单快速、调试方便 | 需安装 .NET | 开发测试 |
| **独立发布** | 无需安装运行时、单文件分发 | 文件较大 (~100MB) | 客户现场部署 |

---

## 🔧 平台特定配置

### Linux 系统服务（systemd）

创建服务文件 `/etc/systemd/system/g2g-admin.service`：

```ini
[Unit]
Description=G2G Admin System
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/g2g-admin
ExecStart=/opt/g2g-admin/G2G.Admin.API
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:5000

[Install]
WantedBy=multi-user.target
```

**启动服务：**
```bash
sudo systemctl daemon-reload
sudo systemctl enable g2g-admin
sudo systemctl start g2g-admin
sudo systemctl status g2g-admin
```

### Windows 服务（使用 NSSM）

```powershell
# 下载 NSSM: https://nssm.cc/download
nssm install G2GAdmin "C:\g2g-admin\G2G.Admin.API.exe"
nssm set G2GAdmin DisplayName "G2G Admin System"
nssm set G2GAdmin StartService SERVICE_AUTO_START
nssm start G2GAdmin
```

---

## 📦 系统要求

### 最低配置
- **CPU:** 1 核心
- **内存:** 512MB
- **磁盘:** 500MB
- **操作系统:** 见上方支持列表

### 推荐配置
- **CPU:** 2 核心
- **内存:** 1GB+
- **磁盘:** 2GB+
- **操作系统:** Ubuntu 22.04 LTS / Windows Server 2022

---

## 🔍 常见问题

### Q: Docker 和直接运行有什么区别？
A: Docker 包含所有依赖，环境隔离；直接运行需要安装 .NET 运行时。

### Q: 可以在 ARM 设备上运行吗？（如树莓派）
A: 可以！使用 Docker 或发布为 `linux-arm` / `linux-arm64`。

### Q: 需要安装 Node.js 吗？
A: 不需要！前端已打包到后端 wwwroot 目录，直接运行后端即可。

### Q: 数据库需要单独安装吗？
A: 不需要！默认使用 SQLite，单文件数据库，开箱即用。可切换 PostgreSQL/MySQL。

---

## 📞 技术支持

- GitHub: https://github.com/sunkejava/g2g-admin
- Issues: https://github.com/sunkejava/g2g-admin/issues
