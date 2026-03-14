#!/bin/bash

echo "=== 重启 G2G Admin 后端服务 ==="

# 停止旧进程
echo "1. 停止旧进程..."
pkill -f "G2G.Admin.API" 2>/dev/null
pkill -f "dotnet.*5000" 2>/dev/null
sleep 2

# 检查是否还有进程在运行
if pgrep -f "G2G.Admin.API" > /dev/null; then
    echo "⚠️  仍有进程在运行，强制终止..."
    pkill -9 -f "G2G.Admin.API"
    sleep 1
fi

# 启动新进程
echo "2. 启动新服务..."
cd /home/sunke/.openclaw/workspace/g2g-admin/G2G.Admin.API

# 设置环境变量（如果需要）
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:5000

# 后台运行
nohup /home/sunke/.dotnet/dotnet run --urls="http://0.0.0.0:5000" > /tmp/g2g-backend.log 2>&1 &

echo "3. 等待服务启动..."
sleep 10

# 检查服务是否正常
if curl -s http://localhost:5000/api/monitor/health > /dev/null; then
    echo "✅ 服务启动成功！"
    echo ""
    echo "访问地址:"
    echo "  后端 API: http://localhost:5000"
    echo "  Swagger:  http://localhost:5000/swagger"
    echo ""
    echo "日志文件：/tmp/g2g-backend.log"
else
    echo "❌ 服务启动失败，请检查日志："
    tail -30 /tmp/g2g-backend.log
fi
