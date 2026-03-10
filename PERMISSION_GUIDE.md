# 🔐 G2G 后台管理系统 - 权限配置指南

## 权限控制架构

### 基于角色的访问控制 (RBAC)

```
用户 (User) 
  ↓ (多对多)
角色 (Role)
  ↓ (多对多)
菜单/权限 (Menu)
```

### 权限控制流程

```
1. 用户登录 
   ↓
2. 获取用户角色
   ↓
3. 获取角色关联的菜单
   ↓
4. 前端根据菜单渲染导航
   ↓
5. 路由守卫检查权限
```

---

## 配置角色权限

### 步骤 1: 创建角色

1. 登录系统（使用管理员账号）
2. 进入 **角色管理** 页面
3. 点击 **新增角色**
4. 填写角色信息：
   - 角色名：如 `普通用户`、`审计员`
   - 描述：如 `只能查看数据，不能修改`

### 步骤 2: 分配菜单权限

1. 在角色列表中找到刚创建的角色
2. 点击 **权限配置** 按钮
3. 在弹出的树形菜单中勾选该角色可访问的菜单
4. 点击 **保存**

### 步骤 3: 分配用户到角色

1. 进入 **用户管理** 页面
2. 找到需要分配角色的用户
3. 点击 **编辑**
4. 在 **角色** 下拉框中选择一个或多个角色
5. 点击 **确定**

---

## 菜单代码说明

| 菜单代码 | 菜单名称 | 路由路径 | 说明 |
|---------|---------|---------|------|
| `dashboard` | 首页 | `/dashboard` | 系统首页，显示统计数据 |
| `users` | 用户管理 | `/users` | 管理用户账号、角色分配 |
| `roles` | 角色管理 | `/roles` | 管理角色和权限配置 |
| `versions` | 版本管理 | `/versions` | 管理应用版本发布 |
| `logs` | 日志管理 | `/logs` | 查看操作日志、系统日志 |
| `settings` | 系统配置 | `/settings` | 系统参数配置 |
| `monitor` | 监控面板 | `/monitor` | 系统资源监控 |

---

## 常见权限场景配置

### 场景 1: 只读用户（审计员）

**需求**: 只能查看数据，不能修改

**配置**:
1. 创建角色：`审计员`
2. 分配菜单：
   - ✅ 首页 (dashboard)
   - ✅ 日志管理 (logs)
   - ✅ 监控面板 (monitor)
   - ❌ 用户管理 (users) - 不勾选
   - ❌ 角色管理 (roles) - 不勾选
   - ❌ 版本管理 (versions) - 不勾选
   - ❌ 系统配置 (settings) - 不勾选
3. 将用户分配到 `审计员` 角色

**效果**:
- 侧边栏只显示首页、日志管理、监控面板
- 尝试访问 `/users` 会跳转到 403 页面

---

### 场景 2: 普通管理员

**需求**: 可以管理用户和版本，不能修改角色和系统配置

**配置**:
1. 创建角色：`普通管理员`
2. 分配菜单：
   - ✅ 首页 (dashboard)
   - ✅ 用户管理 (users)
   - ✅ 版本管理 (versions)
   - ✅ 日志管理 (logs)
   - ❌ 角色管理 (roles) - 不勾选
   - ❌ 系统配置 (settings) - 不勾选
   - ✅ 监控面板 (monitor)
3. 将用户分配到 `普通管理员` 角色

---

### 场景 3: 超级管理员

**需求**: 拥有所有权限

**配置**:
1. 使用默认的 `admin` 角色（或创建新角色）
2. 分配所有菜单：
   - ✅ 首页 (dashboard)
   - ✅ 用户管理 (users)
   - ✅ 角色管理 (roles)
   - ✅ 版本管理 (versions)
   - ✅ 日志管理 (logs)
   - ✅ 系统配置 (settings)
   - ✅ 监控面板 (monitor)

---

## 技术实现细节

### 前端权限控制

#### 1. 登录时获取菜单

```typescript
// Login.vue
const result = await authApi.login(username, password);
const menus = await menuApi.getMyMenus();
localStorage.setItem('userMenus', JSON.stringify(menus));
```

#### 2. 路由守卫检查

```typescript
// router/index.ts
router.beforeEach((to, from, next) => {
  const userMenus = JSON.parse(localStorage.getItem('userMenus') || '[]');
  
  if (to.meta?.code) {
    const hasPermission = userMenus.some(menu => menu.code === to.meta?.code);
    if (!hasPermission) {
      next({ path: '/403', replace: true });
      return;
    }
  }
  next();
});
```

#### 3. 动态菜单渲染

```vue
<!-- MainLayout.vue -->
<el-menu-item 
  v-for="menu in visibleMenus" 
  :key="menu.id" 
  :index="menu.path"
>
  <span>{{ menu.name }}</span>
</el-menu-item>
```

### 后端权限控制

#### 获取用户菜单

```csharp
// MenusController.cs
[HttpGet("my")]
public async Task<IActionResult> GetMyMenus()
{
    var userId = GetCurrentUserId();
    var menus = await _menuService.GetMenusByUserIdAsync(userId);
    return Ok(menus);
}

// MenuService.cs
public async Task<List<Menu>> GetMenusByUserIdAsync(int userId)
{
    return await _dbContext.UserRoles
        .Where(ur => ur.UserId == userId)
        .Join(_dbContext.RoleMenus, ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
        .Distinct()
        .Join(_dbContext.Menus, menuId => menuId, m => m.Id, (menuId, m) => m)
        .OrderBy(m => m.Sort)
        .ToListAsync();
}
```

---

## 安全建议

### 1. 前后端双重验证

- ✅ 前端：路由守卫 + 菜单过滤
- ✅ 后端：API 需要 JWT 认证 + 权限检查
- ⚠️ 注意：前端权限控制仅为用户体验，后端必须验证

### 2. 最小权限原则

- 默认不给任何权限
- 按需分配最小必要权限
- 定期审查权限分配

### 3. 敏感操作二次验证

对于以下操作，建议添加二次验证：
- 删除用户
- 修改角色权限
- 系统配置修改
- 数据导出

### 4. 审计日志

- 记录所有权限变更操作
- 记录用户登录/登出
- 定期审查异常访问

---

## 故障排查

### 问题 1: 用户登录后看不到菜单

**检查项**:
- [ ] 用户是否分配了角色？
- [ ] 角色是否关联了菜单？
- [ ] 浏览器控制台是否有错误？
- [ ] localStorage 中是否有 `userMenus`？

**解决方法**:
```javascript
// 在浏览器控制台检查
console.log(localStorage.getItem('userMenus'));
// 如果为 null，重新登录
```

---

### 问题 2: 提示 403 无权限

**原因**: 用户尝试访问未授权的页面

**解决方法**:
1. 检查用户角色配置
2. 确认角色已关联对应菜单
3. 重新登录刷新权限

---

### 问题 3: 修改权限后不生效

**原因**: 前端缓存了旧权限数据

**解决方法**:
1. 用户重新登录
2. 或清除 localStorage:
```javascript
localStorage.removeItem('userMenus');
location.reload();
```

---

## API 接口

### 获取我的菜单

```http
GET /api/menus/my
Authorization: Bearer <token>
```

**响应**:
```json
[
  {
    "id": 1,
    "code": "dashboard",
    "name": "首页",
    "path": "/dashboard",
    "icon": "UserFilled",
    "sort": 1
  },
  {
    "id": 2,
    "code": "users",
    "name": "用户管理",
    "path": "/users",
    "icon": "User",
    "sort": 2
  }
]
```

---

**最后更新**: 2026-03-10
