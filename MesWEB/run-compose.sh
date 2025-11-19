#!/bin/bash
set -e

echo "========================================"
echo "Crystal Growth Notebook 2 - Docker Compose"
echo "========================================"

# /mnt/d/MSSQL/Dataディレクトリを作成（WSL2の場合）
echo "[0/4] Preparing SQL Server data directory..."
if [ ! -d "/mnt/d/MSSQL/Data" ]; then
    echo "Creating /mnt/d/MSSQL/Data..."
    sudo mkdir -p /mnt/d/MSSQL/Data
    sudo chmod 777 /mnt/d/MSSQL/Data
    echo "Directory created successfully"
else
    echo "Directory already exists: /mnt/d/MSSQL/Data"
fi

# コンテナ用の設定ファイルをコピー
echo ""
echo "[1/4] Preparing configuration..."
cd "$(dirname "$0")/CrystalGrowthNotebook2"
cp -f appsettings.Container.json appsettings.Production.json

# ビルド
echo ""
echo "[2/4] Building application..."
dotnet publish -c Release -o bin/Release/net9.0/publish

cd ..

# 既存のコンテナを停止・削除
echo ""
echo "[3/4] Stopping existing containers..."
docker-compose down 2>/dev/null || true

# Docker Composeで起動
echo ""
echo "[4/4] Starting containers..."
docker-compose up -d

echo ""
echo "========================================"
echo "SUCCESS! Containers started!"
echo "========================================"
echo ""
echo "SQL Server:"
echo "  Host: localhost"
echo "  Port: 1433"
echo "  User: sa"
echo "  Password: YourStrong!Passw0rd123"
echo "  Database: CGNotes (自動作成)"
echo "  Data Location: D:\MSSQL\Data (or /mnt/d/MSSQL/Data in WSL)"
echo ""
echo "Application:"
echo "  Local:  http://localhost:8080"
echo ""
echo "Useful commands:"
echo "  docker-compose logs -f              # View all logs"
echo "  docker-compose logs -f sqlserver      # View SQL Server logs"
echo "  docker-compose logs -f crystalgrowthnotebook2 # View app logs"
echo "  docker-compose stop            # Stop all containers"
echo "  docker-compose start             # Start all containers"
echo "  docker-compose down       # Stop and remove all containers"
echo ""
echo "SQL Server data is stored in: D:\MSSQL\Data"
