# 🌐 网络访问配置指南

## 前端访问方式

### 开发环境启动命令

| 命令 | 访问地址 | 说明 |
|------|---------|------|
| `npm run dev` | http://localhost:5173<br>http://192.168.x.x:5173 | 默认支持本地 + 局域网 |
| `npm run dev:lan` | http://0.0.0.0:5173<br>http://192.168.x.x:5173 | 显式指定局域网访问 |
| `npm run dev:local` | http://localhost:5173 | 仅本地访问 |
| `npm run preview` | http://0.0.0.0:4173 | 生产预览（局域网） |

### 查看本机 IP 地址

**Linux**:
```bash
ip addr show | grep "inet " | grep -v 127.0.0.1
# 或
hostname -I
```

**Windows**:
```cmd
ipconfig
```

**macOS**:
```bash
ifconfig | grep "inet " | grep -v 127.0.0.1
```

### 局域网访问示例

假设你的本机 IP 是 `192.168.1.100`：

1. **启动前端**:
   ```bash
   cd frontend
   npm run dev
   ```

2. **访问地址**:
   - 本机：http://localhost:5173
   - 同一局域网的其他设备：http://192.168.1.100:5173

3. **手机/平板访问**:
   - 确保手机连接到同一 WiFi
   - 浏览器输入：http://192.168.1.100:5173

---

## 后端访问方式

### 开发环境启动

```bash
cd G2G.Admin.API
dotnet run --urls="http://0.0.0.0:5000"
# 或
dotnet watch run --urls="http://0.0.0.0:5000"
```

### 访问地址

| 设备 | 地址 |
|------|------|
| 本机 | http://localhost:5000 |
| 局域网 | http://192.168.x.x:5000 |

### Swagger UI

- 本机：http://localhost:5000/swagger
- 局域网：http://192.168.x.x:5000/swagger

---

## Docker 部署网络配置

### docker-compose.yml

```yaml
version: '3.8'

services:
  frontend:
    build: ./frontend
    ports:
      - "80:80"  # 映射到主机 80 端口
    networks:
      - g2g-network
  
  backend:
    build: ./G2G.Admin.API
    ports:
      - "5000:5000"
    networks:
      - g2g-network

networks:
  g2g-network:
    driver: bridge
```

### 访问 Docker 部署的应用

```bash
# 启动服务
docker-compose up -d

# 查看本机 IP
hostname -I

# 访问地址
# http://192.168.x.x/ (前端)
# http://192.168.x.x:5000/ (后端 API)
```

---

## 防火墙配置

### Linux (UFW)

```bash
# 允许 5173 端口（前端开发）
sudo ufw allow 5173/tcp

# 允许 5000 端口（后端 API）
sudo ufw allow 5000/tcp

# 允许 80 端口（生产环境）
sudo ufw allow 80/tcp

# 查看状态
sudo ufw status
```

### Windows (防火墙)

```powershell
# 允许 5173 端口
netsh advfirewall firewall add rule name="Vite Dev" dir=in action=allow protocol=TCP localport=5173

# 允许 5000 端口
netsh advfirewall firewall add rule name=".NET Backend" dir=in action=allow protocol=TCP localport=5000
```

### macOS (防火墙)

系统偏好设置 → 安全性与隐私 → 防火墙 → 防火墙选项

添加以下应用：
- Node.js (前端)
- dotnet (后端)

---

## 常见问题排查

### 1. 局域网无法访问

**检查项**:
- [ ] 确认启动时使用了 `--host 0.0.0.0`
- [ ] 确认防火墙已开放对应端口
- [ ] 确认设备在同一局域网（同一 WiFi）
- [ ] 确认 IP 地址正确

**测试命令**:
```bash
# 测试端口是否监听
netstat -tlnp | grep 5173

# 测试连通性
ping 192.168.x.x
telnet 192.168.x.x 5173
```

### 2. 跨域问题

如果前端和后端不在同一台机器：

**方案 1**: 修改前端代理配置
```typescript
// vite.config.ts
server: {
  proxy: {
    '/api': {
      target: 'http://192.168.x.x:5000', // 改为后端实际 IP
      changeOrigin: true
    }
  }
}
```

**方案 2**: 后端配置 CORS
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### 3. HTTPS 访问

开发环境使用 HTTP 即可，生产环境建议配置 HTTPS：

**Nginx 配置**:
```nginx
server {
    listen 443 ssl;
    server_name your-domain.com;
    
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    
    location / {
        proxy_pass http://localhost:5173;
    }
}
```

---

## 安全建议

### 开发环境
- ✅ 仅限内网访问
- ✅ 不要暴露到公网
- ✅ 使用强密码保护数据库

### 生产环境
- ✅ 配置 HTTPS
- ✅ 限制 API 访问频率
- ✅ 启用防火墙白名单
- ✅ 定期更新依赖包
- ✅ 监控异常访问

---

**最后更新**: 2026-03-10
