#!/bin/bash

BASE_URL="http://localhost:5000"
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltc29uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltc29uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltc2VtYWlsYWRkcmVzcyI6ImFkbWluQGcyZy5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiQWRtaW4iLCJVc2VyIl0sImV4cCI6MTc3MzQ2ODY3NywiaXNzIjoiRzJHQWRtaW4iLCJhdWQiOiJHMkdBZG1pblVzZXIifQ.ma4hYIP_0pdoLXU_9psBNjBODlu9HXtfSI-CbgV8SxE"

echo "=== 测试版本上传 ==="

# 创建测试文件
cd /home/sunke/.openclaw/workspace/g2g-admin
echo "This is a test file for version upload" > test-version.zip

# 上传
echo "上传文件..."
RESPONSE=$(curl -s -X POST "$BASE_URL/api/versions/upload" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@test-version.zip" \
  -F "versionNo=1.0.1-test" \
  -F "releaseNotes=API 测试上传 - 验证原始文件名")

echo "响应:"
echo "$RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$RESPONSE"

# 提取 ID
NEW_ID=$(echo "$RESPONSE" | grep -o '"id":[0-9]*' | cut -d':' -f2)
echo ""
echo "新版本 ID: $NEW_ID"

if [ -n "$NEW_ID" ]; then
    echo ""
    echo "=== 验证原始文件名 ==="
    DETAIL=$(curl -s "$BASE_URL/api/versions/$NEW_ID" -H "Authorization: Bearer $TOKEN")
    echo "$DETAIL" | python3 -m json.tool 2>/dev/null || echo "$DETAIL"
    
    echo ""
    echo "=== 测试匿名下载 ==="
    curl -sI "$BASE_URL/api/versions/download/$NEW_ID" | head -10
fi

# 清理
rm -f test-version.zip
