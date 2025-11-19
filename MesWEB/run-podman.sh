#!/bin/bash
set -e

echo "========================================"
echo "Crystal Growth Notebook 2 - Podman Build"
echo "========================================"

# ローカルでビルド&パブリッシュ
echo ""
echo "[Step 1/3] Building and publishing locally..."
cd "$(dirname "$0")/CrystalGrowthNotebook2"

# appsettings.jsonの存在確認
if [ ! -f "appsettings.json" ]; then
    echo ""
    echo "ERROR: appsettings.json が見つかりません！"
    echo ""
    echo "以下のコマンドでテンプレートからコピーしてください:"
    echo "  cp appsettings.json.template appsettings.json"
    echo ""
    echo "その後、appsettings.json を編集してSQL Server接続情報を設定してください。"
    echo ""
    exit 1
fi

dotnet publish -c Release -o bin/Release/net9.0/publish

echo ""
echo "[Step 2/3] Building Podman image..."
podman build -f Dockerfile.simple -t crystalgrowthnotebook2:latest .

cd ..

# 既存のコンテナを停止・削除
echo ""
echo "[Step 3/3] Starting container..."
podman rm -f crystalgrowthnotebook2 2>/dev/null || true

# コンテナを実行（すべてのネットワークインターフェースでリッスン）
podman run -d \
  --name crystalgrowthnotebook2 \
  -p 0.0.0.0:8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  --restart=unless-stopped \
  crystalgrowthnotebook2:latest

echo ""
echo "========================================"
echo "SUCCESS! Container started!"
echo "========================================"
echo ""
echo "Access the application at: http://localhost:8080"
echo ""
echo "注: SQL Server接続設定は appsettings.json で管理されています"
echo ""
echo "Useful commands:"
echo "  podman logs -f crystalgrowthnotebook2  # View logs"
echo "  podman stop crystalgrowthnotebook2     # Stop container"
echo "  podman start crystalgrowthnotebook2    # Start container"
echo "  podman rm -f crystalgrowthnotebook2    # Remove container"
