#!/bin/bash

BASE_URL="http://localhost:5000"

echo "=== G2G Admin 版本管理 API 测试 ==="
echo ""

# 1. 登录获取 Token
echo "1️⃣  登录获取 Token..."
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}')

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "❌ 登录失败"
    exit 1
fi
echo "✅ Token 获取成功：${TOKEN:0:50}..."
echo ""

# 2. 测试匿名接口 - 获取最新版本
echo "2️⃣  测试 GET /api/versions/upgrade/latest (匿名)..."
curl -s "$BASE_URL/api/versions/upgrade/latest" | head -c 200
echo ""
echo ""

# 3. 测试匿名接口 - 下载（检查响应头）
echo "3️⃣  测试 GET /api/versions/download/1 (匿名)..."
curl -sI "$BASE_URL/api/versions/download/1" | head -5
echo ""

# 4. 测试需要登录的接口 - 获取列表
echo "4️⃣  测试 GET /api/versions (需要登录)..."
curl -s "$BASE_URL/api/versions" -H "Authorization: Bearer $TOKEN" | head -c 300
echo ""
echo ""

# 5. 测试需要登录的接口 - 获取详情
echo "5️⃣  测试 GET /api/versions/1 (需要登录)..."
curl -s "$BASE_URL/api/versions/1" -H "Authorization: Bearer $TOKEN" | head -c 300
echo ""
echo ""

# 6. 测试需要登录的接口 - 获取当前版本
echo "6️⃣  测试 GET /api/versions/current (需要登录)..."
curl -s "$BASE_URL/api/versions/current" -H "Authorization: Bearer $TOKEN" | head -c 300
echo ""
echo ""

echo "=== 测试完成 ==="
