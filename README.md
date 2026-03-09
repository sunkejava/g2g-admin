# G2G 后台管理系统

> 基于 .NET 10 + Vue 3 的企业级后台管理系统

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3.4-4FC08D?logo=vue.js)](https://vuejs.org/)
[![Element Plus](https://img.shields.io/badge/Element_Plus-2.5-409EFF?logo=element)](https://element-plus.org/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

## 📋 项目简介

G2G 后台管理系统是一套现代化的企业级后台管理解决方案，采用前后端分离架构，提供用户管理、角色权限、版本管理、日志管理、系统监控等核心功能。

### ✨ 特性

- 🚀 **高性能** - .NET 10 + EF Core，响应迅速
- 🎨 **美观界面** - Element Plus + ECharts，视觉体验优秀
- 🔐 **安全可靠** - JWT 认证 + 角色权限控制
- 📦 **容器化** - Docker Compose 一键部署
- 🔄 **CI/CD** - GitHub Actions 自动化构建部署
- 📱 **响应式** - 适配各种屏幕尺寸

## 🛠️ 技术栈

### 后端
| 技术 | 版本 | 说明 |
|------|------|------|
| .NET | 10.0 | WebAPI 框架 |
| EF Core | 10.0 | ORM 框架 |
| SQLite | 3.x | 数据库 (可切换 PostgreSQL/MySQL) |
| JWT | - | Token 认证 |
| BCrypt | 4.1 | 密码加密 |
| Serilog | 10.0 | 日志记录 |
| EPPlus | 8.5 | Excel 导出 |
| Swagger | 10.1 | API 文档 |

### 前端
| 技术 | 版本 | 说明 |
|------|------|------|
| Vue | 3.4 | 渐进式框架 |
| Vite | 7.x | 构建工具 |
| Element Plus | 2.5 | UI 组件库 |
| ECharts | 5.x | 图表库 |
| Pinia | 2.x | 状态管理 |
| Vue Router | 4.x | 路由管理 |
| Axios | 1.x | HTTP 客户端 |

## 📦 功能清单

| 模块 | 功能 | 状态 |
|------|------|------|
| 认证 | 登录/登出、JWT Token、密码加密 | ✅ |
| 用户管理 | CRUD、密码重置、状态切换、多角色分配 | ✅ |
| 角色管理 | CRUD、菜单权限配置 | ✅ |
| 菜单管理 | 树形菜单、权限控制 | ✅ |
| 版本管理 | 上传、检查、对比、回滚、下载 | ✅ |
| 日志管理 | 3 类日志、搜索、Excel 导出、归档 | ✅ |
| 系统配置 | CRUD、缓存清除 | ✅ |
| 监控面板 | CPU/内存/磁盘监控、健康检查 | ✅ |
| 首页仪表盘 | 统计卡片、ECharts 图表 | ✅ |

## 🚀 快速启动

### 环境要求

- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose (可选)

### 方式一：Docker Compose (推荐)

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

访问地址：
- 前端：http://localhost
- 后端：http://localhost:5000
- Swagger：http://localhost:5000/swagger

### 方式二：手动启动

#### 1. 启动后端

```bash
cd G2G.Admin.API

# 还原依赖
dotnet restore

# 运行
dotnet run --urls="http://0.0.0.0:5000"
```

#### 2. 启动前端

```bash
cd frontend

# 安装依赖
npm install

# 开发模式
npm run dev

# 或生产预览
npm run preview
```

访问地址：
- 前端：http://localhost:5173 (开发) / http://localhost:4173 (预览)
- 后端：http://localhost:5000

## 📋 默认账户

```
用户名：admin
密码：admin123
邮箱：admin@g2g.com
角色：系统管理员
```

## 🗂️ 项目结构

```
g2g-admin/
├── G2G.Admin.API/          # 后端项目
│   ├── Controllers/        # API 控制器
│   │   ├── AuthController.cs      # 认证
│   │   ├── UsersController.cs     # 用户
│   │   ├── RolesController.cs     # 角色
│   │   ├── MenusController.cs     # 菜单
│   │   ├── VersionsController.cs  # 版本
│   │   ├── LogsController.cs      # 日志
│   │   ├── SettingsController.cs  # 配置
│   │   ├── MonitorController.cs   # 监控
│   │   └── DashboardController.cs # 仪表盘
│   ├── Services/           # 业务服务
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   ├── RoleService.cs
│   │   ├── MenuService.cs
│   │   ├── VersionService.cs
│   │   ├── LogService.cs
│   │   ├── SettingService.cs
│   │   ├── MonitorService.cs
│   │   └── DashboardService.cs
│   ├── Entities/           # 数据模型
│   ├── Data/               # DbContext
│   ├── Models/             # DTO
│   ├── Program.cs          # 启动配置
│   └── appsettings.json    # 配置文件
├── frontend/               # 前端项目
│   ├── src/
│   │   ├── api/            # API 封装
│   │   ├── views/          # 页面组件
│   │   ├── layouts/        # 布局组件
│   │   ├── router/         # 路由配置
│   │   └── main.ts         # 入口文件
│   └── public/
├── .github/workflows/      # CI/CD 配置
├── docker-compose.yml      # Docker 编排
├── README.md               # 项目说明
└── PUSH_GUIDE.md          # 推送指南
```

## 🔌 API 接口

### 认证接口
| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/auth/login` | 登录 |
| POST | `/api/auth/logout` | 登出 |

### 用户接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/users` | 用户列表 |
| GET | `/api/users/{id}` | 用户详情 |
| POST | `/api/users` | 创建用户 |
| PUT | `/api/users/{id}` | 更新用户 |
| DELETE | `/api/users/{id}` | 删除用户 |
| POST | `/api/users/{id}/reset-password` | 重置密码 |
| PATCH | `/api/users/{id}/toggle-status` | 切换状态 |

### 角色接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/roles` | 角色列表 |
| GET | `/api/roles/{id}` | 角色详情 |
| POST | `/api/roles` | 创建角色 |
| PUT | `/api/roles/{id}` | 更新角色 |
| DELETE | `/api/roles/{id}` | 删除角色 |
| POST | `/api/roles/{id}/menus` | 分配菜单权限 |

### 版本接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/versions` | 版本列表 |
| POST | `/api/versions/upload` | 上传版本 |
| POST | `/api/versions/{id}/rollback` | 版本回滚 |
| GET | `/api/versions/check/{version}` | 检查更新 |
| GET | `/api/versions/compare/{id1}/{id2}` | 版本对比 |

### 日志接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/logs/operation` | 操作日志 |
| GET | `/api/logs/system` | 系统日志 |
| GET | `/api/logs/login` | 登录日志 |
| GET | `/api/logs/export/{type}` | 导出 Excel |

### 监控接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/monitor/system` | 系统资源 |
| GET | `/api/monitor/health` | 健康检查 |

### 统计接口
| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/dashboard/stats` | 统计数据 |
| GET | `/api/dashboard/user-trend` | 用户趋势 |
| GET | `/api/dashboard/log-distribution` | 日志分布 |

完整 API 文档：http://localhost:5000/swagger

## 📝 数据库

### 数据表

| 表名 | 说明 |
|------|------|
| Users | 用户表 |
| Roles | 角色表 |
| Menus | 菜单表 |
| UserRoles | 用户角色关联 |
| RoleMenus | 角色菜单关联 |
| Versions | 版本管理 |
| OperationLogs | 操作日志 |
| SystemLogs | 系统日志 |
| LoginLogs | 登录日志 |
| Settings | 系统配置 |

### 切换数据库

当前使用 SQLite，如需切换 PostgreSQL/MySQL：

1. 安装对应 NuGet 包
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
# 或
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

2. 修改 `appsettings.json` 连接字符串
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=g2g_admin;Username=postgres;Password=your_password"
}
```

3. 修改 `Program.cs` 中的 UseSqlite 为 UseNpgsql/UseMySql

## 🔧 开发指南

### 后端开发

```bash
cd G2G.Admin.API

# 添加迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update

# 运行测试
dotnet test
```

### 前端开发

```bash
cd frontend

# 安装依赖
npm install

# 开发模式
npm run dev

# 构建生产版本
npm run build

# 代码检查
npm run lint
```

## 🚢 部署

### 生产环境部署

1. **修改配置**
   - 更新 `appsettings.Production.json` 中的数据库连接
   - 修改 JWT Secret Key
   - 配置 HTTPS

2. **构建发布**
```bash
# 后端
dotnet publish -c Release -o ./publish

# 前端
npm run build
```

3. **Docker 部署**
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### CI/CD

项目已配置 GitHub Actions：
- **CI**: 代码提交自动构建测试
- **CD**: 主分支推送自动构建 Docker 镜像

配置说明：
- `.github/workflows/ci.yml` - 持续集成
- `.github/workflows/cd.yml` - 持续部署

## 🛡️ 安全建议

1. **修改默认密码** - 首次登录后立即修改 admin 密码
2. **配置 HTTPS** - 生产环境必须使用 HTTPS
3. **更新 JWT Secret** - 使用强随机字符串
4. **定期备份数据库** - 建议每日备份
5. **限制 API 访问** - 配置防火墙规则

## 📄 许可证

MIT License

## 👥 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📞 联系方式

- GitHub: https://github.com/sunkejava
- Email: sunke@example.com

---

**Made with ❤️ by sunkejava**
