# G2G Admin 升级指南

## 从旧版本升级

### 自动数据库升级

系统现在支持**自动数据库结构升级**。当你从旧版本（没有 `OriginalFileName` 字段）升级到新版本时：

1. **启动应用时自动执行升级**
   - 程序会检查 `Versions` 表是否有 `OriginalFileName` 列
   - 如果不存在，自动添加该字段
   - 升级过程无需手动干预

2. **升级日志**
   ```
   📦 正在升级 Versions 表...
   ✅ 添加 OriginalFileName 列到 Versions 表
   ```

### 手动升级（可选）

如果自动升级失败，可以手动执行 SQL：

```sql
-- 为 Versions 表添加 OriginalFileName 字段
ALTER TABLE Versions ADD COLUMN OriginalFileName TEXT NOT NULL DEFAULT '';
```

SQLite 数据库位置：`G2G.Admin.API/g2g-admin.db`

---

## 版本变更说明

### v1.1.0 (2026-03-14)

**新增功能：**
- ✅ 版本接口支持匿名访问（无需登录即可检查/下载更新）
- ✅ 下载文件使用原始文件名
- ✅ 版本列表显示文件名列
- ✅ 自动数据库结构升级

**API 变更：**
- `GET /api/versions/upgrade/latest` - 现在支持匿名访问
- `GET /api/versions/download/{id}` - 现在支持匿名访问，返回原始文件名

**数据库变更：**
- `Versions` 表新增 `OriginalFileName` 字段

**升级步骤：**
1. 备份数据库（`g2g-admin.db`）
2. 替换后端程序文件
3. 启动应用（会自动升级数据库）
4. 验证功能正常

---

### v1.0.0 (2026-03-10)

初始版本

---

## 常见问题

### Q: 升级后版本列表无法加载？
A: 检查日志，确认数据库升级是否成功。如果失败，手动执行上面的 SQL 语句。

### Q: 下载文件时返回 404？
A: 检查 `uploads/versions/` 目录是否存在且文件完整。

### Q: 如何备份数据库？
A: 直接复制 `g2g-admin.db` 文件即可。

---

## 技术支持

- GitHub: https://github.com/sunkejava/g2g-admin
- Issues: https://github.com/sunkejava/g2g-admin/issues
