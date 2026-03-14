#!/bin/bash

echo "=== G2G Admin 环境检查 ==="
echo ""

# 检查 Docker
if command -v docker &> /dev/null; then
    echo "✅ Docker: $(docker --version)"
else
    echo "❌ Docker: 未安装"
    echo "   安装：https://docs.docker.com/get-docker/"
fi

# 检查 Docker Compose
if command -v docker-compose &> /dev/null; then
    echo "✅ Docker Compose: $(docker-compose --version)"
elif docker compose version &> /dev/null; then
    echo "✅ Docker Compose: $(docker compose version)"
else
    echo "⚠️  Docker Compose: 未安装（使用 Docker 部署时需要）"
fi

# 检查 .NET
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    if [[ $DOTNET_VERSION == 10.* ]]; then
        echo "✅ .NET: $DOTNET_VERSION"
    else
        echo "⚠️  .NET: $DOTNET_VERSION (需要 .NET 10.x)"
    fi
else
    echo "❌ .NET: 未安装"
    echo "   安装：https://dotnet.microsoft.com/download/dotnet/10.0"
fi

# 检查 Node.js（仅开发需要）
if command -v node &> /dev/null; then
    echo "✅ Node.js: $(node --version)（开发需要）"
else
    echo "ℹ️  Node.js: 未安装（仅开发时需要，生产部署不需要）"
fi

# 检查端口
echo ""
echo "=== 端口占用检查 ==="
for port in 5000 5173; do
    if command -v ss &> /dev/null; then
        if ss -tlnp | grep -q ":$port "; then
            echo "⚠️  端口 $port: 已被占用"
        else
            echo "✅ 端口 $port: 可用"
        fi
    elif command -v netstat &> /dev/null; then
        if netstat -tlnp 2>/dev/null | grep -q ":$port "; then
            echo "⚠️  端口 $port: 已被占用"
        else
            echo "✅ 端口 $port: 可用"
        fi
    fi
done

echo ""
echo "=== 检查完成 ==="
