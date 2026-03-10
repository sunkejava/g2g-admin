@echo off
chcp 65001 >nul
echo ========================================
echo   G2G Admin 一键部署脚本
echo ========================================
echo.

:: 检查 Node.js
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo [错误] 未检测到 Node.js，请先安装 Node.js
    echo 下载地址：https://nodejs.org/
    pause
    exit /b 1
)

:: 检查 .NET
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [错误] 未检测到 .NET SDK，请先安装 .NET 10 SDK
    echo 下载地址：https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/4] 开始构建前端...
cd frontend
call npm install
if %errorlevel% neq 0 (
    echo [错误] 前端依赖安装失败
    pause
    exit /b 1
)

call npm run build
if %errorlevel% neq 0 (
    echo [错误] 前端构建失败
    pause
    exit /b 1
)
cd ..

echo.
echo [2/4] 前端构建完成，文件已输出到 G2G.Admin.API/wwwroot
echo.

echo [3/4] 开始构建后端...
cd G2G.Admin.API
dotnet restore
if %errorlevel% neq 0 (
    echo [错误] 后端还原失败
    pause
    exit /b 1
)

dotnet publish -c Release -o ../publish
if %errorlevel% neq 0 (
    echo [错误] 后端发布失败
    pause
    exit /b 1
)
cd ..

echo.
echo [4/4] 后端发布完成
echo.

echo ========================================
echo   部署完成！
echo ========================================
echo.
echo 启动方式:
echo   1. 直接运行：dotnet G2G.Admin.API.dll
echo   2. 双击：start.bat
echo.
echo 访问地址:
echo   http://localhost:5000
echo   http://localhost:5000/swagger
echo.
echo 默认账号:
echo   用户名：admin
echo   密码：admin123
echo.
pause
