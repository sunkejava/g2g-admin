#!/bin/bash

echo "========================================"
echo "  G2G Admin 启动"
echo "========================================"
echo ""

# 检查是否已发布
if [ ! -f "publish/G2G.Admin.API.dll" ]; then
    echo "[提示] 未检测到发布文件，正在执行首次部署..."
    echo ""
    ./deploy.sh
fi

echo "正在启动 G2G Admin..."
echo ""
echo "访问地址:"
echo "  http://localhost:5000"
echo "  http://localhost:5000/swagger"
echo ""
echo "按 Ctrl+C 停止服务"
echo "========================================"
echo ""

cd publish
dotnet G2G.Admin.API.dll --urls="http://0.0.0.0:5000"
