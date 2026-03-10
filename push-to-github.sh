#!/bin/bash
# G2G Admin 推送脚本
# 使用方法：./push-to-github.sh <your_token>

TOKEN=$1

if [ -z "$TOKEN" ]; then
    echo "用法：$0 <GitHub_Personal_Access_Token>"
    echo ""
    echo "获取 Token 步骤:"
    echo "1. 访问 https://github.com/settings/tokens"
    echo "2. 点击 'Generate new token (classic)'"
    echo "3. 勾选 'repo' 权限"
    echo "4. 生成并复制 Token"
    exit 1
fi

cd /home/sunke/.openclaw/workspace/g2g-admin

# 设置远程仓库 URL (包含 token)
git remote set-url origin https://sunkejava:${TOKEN}@github.com/sunkejava/g2g-admin.git

echo "开始推送到 GitHub..."
git push -u origin master

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ 推送成功！"
    echo "📍 仓库地址：https://github.com/sunkejava/g2g-admin"
else
    echo ""
    echo "❌ 推送失败，请检查 Token 是否正确"
fi
