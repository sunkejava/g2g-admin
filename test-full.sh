#!/bin/bash

BASE_URL="http://localhost:5000"

echo "=== G2G Admin 版本管理完整测试 ==="
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
echo "✅ Token 获取成功"
echo ""

# 2. 测试匿名接口 - 获取最新版本
echo "2️⃣  测试 GET /api/versions/upgrade/latest (匿名)..."
RESULT=$(curl -s "$BASE_URL/api/versions/upgrade/latest")
if echo "$RESULT" | grep -q "versionNo"; then
    echo "✅ 匿名接口正常"
    echo "   响应：$RESULT" | head -c 150
    echo "..."
else
    echo "❌ 匿名接口失败"
fi
echo ""

# 3. 测试需要登录的接口 - 获取列表
echo "3️⃣  测试 GET /api/versions (需要登录)..."
RESULT=$(curl -s "$BASE_URL/api/versions" -H "Authorization: Bearer $TOKEN")
if echo "$RESULT" | grep -q "items"; then
    echo "✅ 列表接口正常"
else
    echo "❌ 列表接口失败"
fi
echo ""

# 4. 测试需要登录的接口 - 获取详情
echo "4️⃣  测试 GET /api/versions/1 (需要登录)..."
RESULT=$(curl -s "$BASE_URL/api/versions/1" -H "Authorization: Bearer $TOKEN")
if echo "$RESULT" | grep -q "originalFileName"; then
    echo "✅ 详情接口正常 (包含 OriginalFileName 字段)"
    echo "   OriginalFileName: $(echo "$RESULT" | grep -o '"originalFileName":"[^"]*"' | cut -d'"' -f4)"
else
    echo "❌ 详情接口失败"
fi
echo ""

# 5. 测试上传
echo "5️⃣  测试 POST /api/versions/upload (上传新版本)..."
cd /home/sunke/.openclaw/workspace/g2g-admin
echo "test version file content" > test-version.zip

UPLOAD_RESULT=$(curl -s -X POST "$BASE_URL/api/versions/upload" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@test-version.zip" \
  -F "versionNo=1.0.1-test" \
  -F "releaseNotes=API 测试上传 - 验证原始文件名")

rm -f test-version.zip

NEW_ID=$(echo "$UPLOAD_RESULT" | grep -o '"id":[0-9]*' | cut -d':' -f2)
if [ -n "$NEW_ID" ]; then
    echo "✅ 上传成功，新版本 ID: $NEW_ID"
    
    # 验证原始文件名
    echo ""
    echo "6️⃣  验证原始文件名保存..."
    DETAIL=$(curl -s "$BASE_URL/api/versions/$NEW_ID" -H "Authorization: Bearer $TOKEN")
    ORIGINAL_NAME=$(echo "$DETAIL" | grep -o '"originalFileName":"[^"]*"' | cut -d'"' -f4)
    echo "   原始文件名：$ORIGINAL_NAME"
    
    if [ "$ORIGINAL_NAME" = "test-version.zip" ]; then
        echo "✅ 原始文件名保存正确！"
    else
        echo "⚠️  原始文件名：$ORIGINAL_NAME (期望：test-version.zip)"
    fi
    
    # 7. 测试匿名下载
    echo ""
    echo "7️⃣  测试 GET /api/versions/download/$NEW_ID (匿名下载)..."
    DOWNLOAD_HEADER=$(curl -sI "$BASE_URL/api/versions/download/$NEW_ID")
    if echo "$DOWNLOAD_HEADER" | grep -q "200 OK"; then
        echo "✅ 匿名下载接口正常"
        FILENAME=$(echo "$DOWNLOAD_HEADER" | grep -i "filename=" | head -1)
        echo "   下载文件名：$FILENAME"
    else
        echo "❌ 匿名下载失败"
        echo "$DOWNLOAD_HEADER" | head -5
    fi
    
    # 8. 测试删除
    echo ""
    echo "8️⃣  测试 DELETE /api/versions/$NEW_ID (删除测试版本)..."
    DELETE_RESULT=$(curl -s -X DELETE "$BASE_URL/api/versions/$NEW_ID" -H "Authorization: Bearer $TOKEN")
    if echo "$DELETE_RESULT" | grep -q "删除成功"; then
        echo "✅ 删除成功"
    else
        echo "⚠️  删除响应：$DELETE_RESULT"
    fi
else
    echo "❌ 上传失败"
    echo "$UPLOAD_RESULT"
fi

echo ""
echo "=== 测试完成 ==="
