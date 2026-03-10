# 阶段 1: 构建前端
FROM node:20-alpine AS frontend-build
WORKDIR /frontend

# 复制前端源码
COPY ../frontend/package*.json ./
RUN npm ci --only=production

COPY ../frontend/ ./
RUN npm run build

# 阶段 2: 构建后端
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# 复制后端项目文件
COPY *.csproj .
RUN dotnet restore

# 复制后端源码
COPY . .

# 复制前端构建结果到 wwwroot
COPY --from=frontend-build /frontend/dist ./wwwroot

# 发布后端（包含前端）
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 阶段 3: 运行环境
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 暴露端口
EXPOSE 8080

# 复制发布文件（包含 wwwroot）
COPY --from=backend-build /app/publish .

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# 启动应用
ENTRYPOINT ["dotnet", "G2G.Admin.API.dll"]
