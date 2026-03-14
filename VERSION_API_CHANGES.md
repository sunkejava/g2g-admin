# 版本接口变更说明

## 变更时间
2026-03-14

## 变更内容

### 1. 数据库变更
- **表**: `Versions`
- **新增字段**: `OriginalFileName` (TEXT) - 存储上传时的原始文件名

### 2. API 接口变更

#### `GET /api/versions/upgrade/latest`
- **变更前**: 需要登录认证
- **变更后**: ✅ 支持匿名访问（添加 `[AllowAnonymous]` 属性）
- **用途**: PC 客户端获取最新版本信息，无需登录即可检查更新

#### `GET /api/versions/download/{id}`
- **变更前**: 需要登录认证，下载文件名为 `{VersionNo}.zip`
- **变更后**: 
  - ✅ 支持匿名访问（添加 `[AllowAnonymous]` 属性）
  - ✅ 下载文件名使用上传时的原始文件名
- **用途**: 客户端直接下载更新文件，无需权限校验

### 3. 代码变更

#### 实体类 (`Entities/Version.cs`)
```csharp
public class AppVersion
{
    // ... 其他属性
    public string OriginalFileName { get; set; } = string.Empty; // 新增
}
```

#### 上传接口 (`VersionsController.cs`)
- 保存上传文件的原始名称到 `OriginalFileName` 字段

#### 下载接口 (`VersionsController.cs`)
- 移除权限校验
- 使用 `OriginalFileName` 作为下载文件名（如果为空则回退到 `{VersionNo}.zip`）

### 4. 使用示例

#### 检查最新版本（无需 Token）
```bash
curl http://localhost:5000/api/versions/upgrade/latest
```

响应示例：
```json
{
  "versionNo": "1.0.2",
  "releaseNotes": "修复了一些 bug",
  "fileSize": 10485760,
  "fileSizeMB": 10.0,
  "downloadUrl": "/api/versions/download/5",
  "uploadedAt": "2026-03-14T12:00:00Z",
  "hasPreviousVersion": true,
  "previousVersionNo": "1.0.1"
}
```

#### 下载文件（无需 Token）
```bash
curl -O http://localhost:5000/api/versions/download/5
```
- 下载的文件名将是上传时的原始文件名（例如：`G2G.Client.v1.0.2.zip`）

## 安全说明
- 这两个接口设计为公开访问，允许 PC 客户端无需认证即可检查和下载更新
- 上传、删除、回滚等管理接口仍然需要登录认证
- 下载操作会记录日志（匿名用户标记为 "anonymous"）

## 向后兼容
- ✅ 现有功能完全兼容
- ✅ 新增字段有默认值，不影响现有数据
- ✅ 旧版本上传的文件如果没有 `OriginalFileName`，下载时会回退到 `{VersionNo}.zip`
