# G2G 后台管理系统 - 开发维护文档

## 📋 目录

1. [开发环境配置](#开发环境配置)
2. [代码规范](#代码规范)
3. [分支管理](#分支管理)
4. [测试指南](#测试指南)
5. [发布流程](#发布流程)
6. [故障排查](#故障排查)

## 开发环境配置

### IDE 推荐

| 用途 | 推荐工具 | 插件 |
|------|----------|------|
| 后端开发 | Visual Studio 2022 / Rider | C#, EF Core Power Tools |
| 前端开发 | VS Code | Volar, ESLint, Prettier |
| 数据库 | DB Browser for SQLite / DBeaver | - |
| API 测试 | Postman / Swagger UI | - |

### VS Code 配置

创建 `.vscode/settings.json`:
```json
{
  "editor.formatOnSave": true,
  "editor.defaultFormatter": "esbenp.prettier-vscode",
  "vue.languageFeatures": {
    "references": true
  },
  "files.associations": {
    "*.vue": "vue"
  }
}
```

### 开发工具

```bash
# .NET 工具
dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-watch

# Node.js 工具
npm install -g npm-check-updates
npm install -g vite
```

## 代码规范

### 命名规范

#### C# 后端

```csharp
// 类名 - PascalCase
public class UserService { }

// 方法名 - PascalCase
public async Task<User> GetUserByIdAsync(int id) { }

// 私有字段 - _camelCase
private readonly G2GDbContext _dbContext;

// 局部变量 - camelCase
var userList = await _dbContext.Users.ToListAsync();

// 常量 - UPPER_CASE
public const int MAX_PAGE_SIZE = 100;

// 接口 - IPrefix
public interface IUserService { }
```

#### Vue 前端

```typescript
// 组件名 - PascalCase
<template>
  <UserList />
</template>

// 变量 - camelCase
const userList = ref([])

// 函数 - camelCase
const handleLogin = async () => {}

// 常量 - UPPER_CASE
const MAX_RETRY_COUNT = 3

// 组件文件 - PascalCase.vue
UserList.vue, RoleForm.vue
```

### 注释规范

```csharp
/// <summary>
/// 用户登录
/// </summary>
/// <param name="request">登录请求</param>
/// <param name="ip">IP 地址</param>
/// <returns>登录结果</returns>
public async Task<LoginResponse?> LoginAsync(LoginRequest request, string ip)
{
    // 验证用户
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    
    // 记录登录日志
    await LogLoginAsync(user, ip);
    
    return new LoginResponse { Token = token };
}
```

### 错误处理

```csharp
try
{
    var user = await _userService.CreateAsync(dto);
    return Ok(user);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "创建用户失败");
    return BadRequest(new { message = "用户名或邮箱已存在" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "未知错误");
    return StatusCode(500, new { message = "服务器错误" });
}
```

## 分支管理

### Git 工作流

```
main (生产)
  ↑
develop (开发)
  ↑
feature/* (功能分支)
  ↑
bugfix/* (修复分支)
```

### 分支命名

| 类型 | 命名格式 | 示例 |
|------|----------|------|
| 功能 | feature/功能名 | feature/user-management |
| 修复 | bugfix/问题描述 | bugfix/login-error |
| 热修复 | hotfix/紧急修复 | hotfix/security-patch |
| 文档 | docs/文档内容 | docs/api-update |

### 提交规范

```bash
# 格式：<type>(<scope>): <subject>

# 类型
feat:     新功能
fix:      修复 bug
docs:     文档更新
style:    代码格式
refactor: 重构
test:     测试
chore:    构建/工具

# 示例
feat(users): 添加用户导出功能
fix(auth): 修复登录 token 过期问题
docs(readme): 更新部署说明
```

### 操作流程

```bash
# 1. 创建功能分支
git checkout -b feature/user-export develop

# 2. 开发并提交
git add .
git commit -m "feat(users): 添加用户导出功能"

# 3. 同步主分支
git fetch origin
git rebase origin/develop

# 4. 推送分支
git push origin feature/user-export

# 5. 创建 Pull Request
# 在 GitHub 上创建 PR，等待审查

# 6. 合并后删除分支
git branch -d feature/user-export
```

## 测试指南

### 后端测试

#### 单元测试

```csharp
// Tests/UserServiceTests.cs
[Fact]
public async Task CreateUser_WithValidData_ReturnsUser()
{
    // Arrange
    var dto = new CreateUserDto 
    { 
        Username = "test", 
        Email = "test@example.com",
        Password = "123456"
    };
    
    // Act
    var user = await _userService.CreateAsync(dto);
    
    // Assert
    Assert.NotNull(user);
    Assert.Equal("test", user.Username);
}

// 运行测试
dotnet test
```

#### 集成测试

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var request = new LoginRequest 
    { 
        Username = "admin", 
        Password = "admin123" 
    };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/login", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    Assert.NotNull(result.Token);
}
```

### 前端测试

```bash
# 安装测试库
npm install -D vitest @vue/test-utils

# 添加测试脚本
# package.json
{
  "scripts": {
    "test": "vitest"
  }
}
```

#### 组件测试

```typescript
// tests/Login.test.ts
import { mount } from '@vue/test-utils'
import Login from '@/views/Login.vue'

describe('Login.vue', () => {
  it('renders login form', () => {
    const wrapper = mount(Login)
    expect(wrapper.find('input[type="text"]').exists()).toBe(true)
    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
  })

  it('submits form with valid data', async () => {
    const wrapper = mount(Login)
    await wrapper.find('input[type="text"]').setValue('admin')
    await wrapper.find('input[type="password"]').setValue('admin123')
    await wrapper.find('button[type="submit"]').trigger('click')
    
    expect(wrapper.emitted()).toHaveProperty('login')
  })
})
```

## 发布流程

### 版本号规范

遵循 Semantic Versioning (语义化版本):

```
主版本号。次版本号。修订号
  ↑      ↑      ↑
重大变更  新功能  Bug 修复

示例：
1.0.0  - 初始发布
1.1.0  - 添加新功能
1.1.1  - Bug 修复
2.0.0  - 破坏性变更
```

### 发布检查清单

#### 发布前

- [ ] 所有测试通过
- [ ] 代码审查完成
- [ ] 更新 CHANGELOG.md
- [ ] 更新版本号
- [ ] 检查配置文件
- [ ] 备份数据库

#### 发布中

- [ ] 构建生产版本
- [ ] 运行迁移脚本
- [ ] 部署到服务器
- [ ] 验证服务状态

#### 发布后

- [ ] 功能验证
- [ ] 监控日志
- [ ] 性能测试
- [ ] 用户通知

### 自动化发布

```bash
# scripts/release.sh
#!/bin/bash

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "用法：$0 <版本号>"
    exit 1
fi

echo "发布版本：$VERSION"

# 更新版本号
# (手动更新 csproj 和 package.json)

# 构建
dotnet publish -c Release
npm run build

# 创建 Git 标签
git tag -a "v$VERSION" -m "Release version $VERSION"
git push origin "v$VERSION"

# 创建 GitHub Release
# (手动在 GitHub 创建)

echo "发布完成！"
```

## 故障排查

### 日志分析

#### 后端日志位置

```bash
# 开发环境
G2G.Admin.API/logs/g2g-admin-*.log

# Docker
docker logs g2g-backend

# Systemd
journalctl -u g2g-backend -f
```

#### 日志级别

| 级别 | 说明 | 何时使用 |
|------|------|----------|
| Trace | 详细跟踪 | 调试时 |
| Debug | 调试信息 | 开发时 |
| Information | 一般信息 | 正常运行 |
| Warning | 警告 | 潜在问题 |
| Error | 错误 | 操作失败 |
| Critical | 严重错误 | 系统崩溃 |

### 常见问题排查

#### 1. 登录失败

```bash
# 检查数据库
sqlite3 G2G.Admin.API/g2g-admin.db "SELECT * FROM Users;"

# 检查日志
tail -f logs/g2g-admin-*.log | grep "登录"

# 重置密码
# 运行脚本或手动更新数据库
```

#### 2. API 返回 500

```bash
# 查看详细错误
curl -v http://localhost:5000/api/users

# 检查 Swagger
http://localhost:5000/swagger

# 查看应用日志
tail -100 logs/g2g-admin-*.log
```

#### 3. 前端页面空白

```bash
# 检查浏览器控制台
F12 -> Console

# 检查网络请求
F12 -> Network

# 清除缓存
Ctrl+Shift+Delete

# 重新构建
npm run build
```

#### 4. 数据库迁移失败

```bash
# 查看迁移状态
dotnet ef migrations list

# 删除迁移
dotnet ef migrations remove

# 重新添加
dotnet ef migrations add InitialCreate

# 强制更新
dotnet ef database update --force
```

### 性能问题排查

#### 1. 响应慢

```bash
# 检查数据库查询
# 在 Program.cs 中启用 SQL 日志
options.LogTo(Console.WriteLine, LogLevel.Information)

# 检查慢查询
sqlite3 g2g-admin.db "PRAGMA query_only = OFF;"

# 添加索引
CREATE INDEX IX_Users_Username ON Users(Username);
```

#### 2. 内存泄漏

```bash
# 监控内存
dotnet-counters monitor --process-id <PID>

# 分析 dump
dotnet-dump collect --process-id <PID>
dotnet-dump analyze <dump-file>
```

#### 3. CPU 占用高

```bash
# 查看进程
top -p <PID>

# 分析性能
dotnet-trace collect --process-id <PID>
```

## 维护计划

### 日常维护

| 任务 | 频率 | 说明 |
|------|------|------|
| 检查日志 | 每日 | 查看错误和警告 |
| 备份数据库 | 每日 | 自动或手动备份 |
| 监控系统资源 | 每日 | CPU、内存、磁盘 |
| 检查更新 | 每周 | .NET、Node.js 包更新 |

### 定期维护

| 任务 | 频率 | 说明 |
|------|------|------|
| 清理旧日志 | 每月 | 归档或删除 30 天前日志 |
| 数据库优化 | 每月 | VACUUM、重建索引 |
| 安全更新 | 每月 | 更新依赖包 |
| 性能测试 | 每季度 | 压力测试、瓶颈分析 |

### 备份策略

```bash
#!/bin/bash
# scripts/backup.sh

BACKUP_DIR="/backup/g2g-admin"
DATE=$(date +%Y%m%d_%H%M%S)

# 备份数据库
cp G2G.Admin.API/g2g-admin.db $BACKUP_DIR/g2g-admin_$DATE.db

# 备份配置文件
cp G2G.Admin.API/appsettings.Production.json $BACKUP_DIR/

# 压缩
tar -czf $BACKUP_DIR/backup_$DATE.tar.gz $BACKUP_DIR/*

# 删除 30 天前的备份
find $BACKUP_DIR -name "backup_*.tar.gz" -mtime +30 -delete

echo "备份完成：backup_$DATE.tar.gz"
```

---

**维护愉快！记得定期备份！** 💾
