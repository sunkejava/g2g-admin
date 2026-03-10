#!/bin/bash

echo "========================================"
echo "  G2G Admin 一键部署脚本"
echo "========================================"
echo ""

# 检查 Node.js
if ! command -v node &> /dev/null; then
    echo "[错误] 未检测到 Node.js，请先安装 Node.js"
    echo "下载地址：https://nodejs.org/"
    exit 1
fi

# 检查 .NET
if ! command -v dotnet &> /dev/null; then
    echo "[错误] 未检测到 .NET SDK，请先安装 .NET 10 SDK"
    echo "下载地址：https://dotnet.microsoft.com/download"
    exit 1
fi

echo "[1/4] 开始构建前端..."
cd frontend
npm install
if [ $? -ne 0 ]; then
    echo "[错误] 前端依赖安装失败"
    exit 1
fi

npm run build
if [ $? -ne 0 ]; then
    echo "[错误] 前端构建失败"
    exit 1
fi
cd ..

echo ""
echo "[2/4] 前端构建完成，文件已输出到 G2G.Admin.API/wwwroot"
ls -la G2G.Admin.API/wwwroot
echo ""

echo "[3/4] 开始构建后端..."
cd G2G.Admin.API
dotnet restore
if [ $? -ne 0 ]; then
    echo "[错误] 后端还原失败"
    exit 1
fi

dotnet publish -c Release -o ../publish
if [ $? -ne 0 ]; then
    echo "[错误] 后端发布失败"
    exit 1
fi
cd ..

echo ""
echo "[4/4] 后端发布完成"
echo ""

echo "========================================"
echo "  部署完成！"
echo "========================================"
echo ""
echo "启动方式:"
echo "  1. 直接运行：dotnet G2G.Admin.API.dll"
echo "  2. 使用脚本：./start.sh"
echo ""
echo "访问地址:"
echo "  http://localhost:5000"
echo "  http://localhost:5000/swagger"
echo ""
echo "默认账号:"
echo "  用户名：admin"
echo "  密码：admin123"
echo ""
