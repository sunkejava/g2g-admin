# 📝 G2G 后台管理系统 - 日志类型说明

## 三种日志的区别

### 1. 操作日志 (OperationLog)

**记录内容**: 用户在系统中的业务操作行为

**触发场景**:
- ✅ 用户管理：创建用户、编辑用户、删除用户、重置密码
- ✅ 角色管理：创建角色、编辑角色、删除角色、分配权限
- ✅ 版本管理：上传版本、回滚版本、删除版本
- ✅ 系统配置：修改系统设置
- ✅ 数据导出：导出 Excel 数据

**记录字段**:
```csharp
public class OperationLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }      // 操作用户 ID
    public string Action { get; set; }     // 操作类型（如：创建用户）
    public string Module { get; set; }     // 所属模块（如：用户管理）
    public string? Details { get; set; }   // 操作详情
    public string? Ip { get; set; }        // 操作 IP
    public DateTime CreatedAt { get; set; }
}
```

**用途**:
- 审计用户操作行为
- 追踪数据变更历史
- 责任追溯
- 合规要求

---

### 2. 系统日志 (SystemLog)

**记录内容**: 系统运行时的技术日志、错误信息

**触发场景**:
- ✅ 应用程序启动/停止
- ✅ 数据库连接成功/失败
- ✅ API 请求处理异常
- ✅ 未捕获的异常和错误
- ✅ 性能警告（如：慢查询）
- ✅ 服务健康检查失败
- ✅ 磁盘空间不足警告

**记录字段**:
```csharp
public class SystemLog
{
    public int Id { get; set; }
    public string Level { get; set; }    // 日志级别：Information, Warning, Error, Critical
    public string Source { get; set; }   // 日志来源（如：Program.cs, UserService）
    public string Message { get; set; }  // 日志消息
    public string? StackTrace { get; set; } // 错误堆栈（仅错误日志）
    public DateTime CreatedAt { get; set; }
}
```

**日志级别**:
| 级别 | 说明 | 示例 |
|------|------|------|
| Information | 一般信息 | "应用程序已启动" |
| Warning | 警告信息 | "磁盘使用率超过 80%" |
| Error | 错误信息 | "数据库连接失败" |
| Critical | 严重错误 | "系统崩溃" |

**用途**:
- 系统故障诊断
- 性能问题排查
- 系统健康监控
- 技术团队运维

---

### 3. 登录日志 (LoginLog)

**记录内容**: 用户登录/登出行为

**触发场景**:
- ✅ 用户登录成功
- ✅ 用户登录失败（密码错误、账号不存在）
- ✅ 用户登出
- ✅ Token 过期/无效
- ✅ 账号被锁定

**记录字段**:
```csharp
public class LoginLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }     // 用户 ID（登录失败时为 null）
    public string Username { get; set; } // 登录用户名
    public bool Success { get; set; }    // 是否成功
    public string? Ip { get; set; }      // 登录 IP
    public string? UserAgent { get; set; } // 浏览器/设备信息
    public DateTime CreatedAt { get; set; }
}
```

**用途**:
- 安全审计
- 检测暴力破解攻击
- 追踪账号异常登录
- 用户行为分析

---

## 三种日志对比表

| 特性 | 操作日志 | 系统日志 | 登录日志 |
|------|---------|---------|---------|
| **记录主体** | 用户 | 系统 | 用户 |
| **触发时机** | 业务操作 | 系统事件 | 登录/登出 |
| **主要用户** | 管理员/审计 | 技术人员 | 安全团队 |
| **保存期限** | 长期（年） | 中期（月） | 长期（年） |
| **查询频率** | 中 | 高 | 中 |
| **安全级别** | 高 | 中 | 高 |

---

## 使用场景示例

### 场景 1：用户反馈数据被误删

**排查步骤**:
1. 查看 **操作日志** → 找到删除操作的记录
2. 确认操作人、操作时间、操作详情
3. 如有必要，查看 **系统日志** → 确认删除时是否有异常

### 场景 2：系统响应变慢

**排查步骤**:
1. 查看 **系统日志** → 寻找 Error/Warning 级别的日志
2. 检查是否有慢查询、连接超时等记录
3. 查看 **操作日志** → 是否有大量并发操作

### 场景 3：账号被盗用

**排查步骤**:
1. 查看 **登录日志** → 检查异常 IP、异常时间的登录
2. 查看 **操作日志** → 确认盗用者进行了哪些操作
3. 查看 **系统日志** → 是否有暴力破解尝试

---

## 最佳实践

### 记录原则

1. **操作日志**: 记录所有变更数据的操作（增删改）
2. **系统日志**: 使用合适的日志级别，避免过度记录
3. **登录日志**: 记录所有登录尝试（成功和失败）

### 性能考虑

1. 日志记录应异步进行，避免阻塞主流程
2. 定期归档旧日志（建议 30 天以上）
3. 对于高频操作，可考虑采样记录

### 安全考虑

1. 日志中包含敏感信息时应脱敏
2. 限制日志访问权限（仅管理员可查看）
3. 防止日志被篡改（可考虑写入独立存储）

---

**最后更新**: 2026-03-10
