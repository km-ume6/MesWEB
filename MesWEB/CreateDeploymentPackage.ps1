# MesWEB 発行後の自動Zip作成スクリプト

$publishPath = ".\bin\Release\net10.0\publish"
$zipDestination = "\\192.168.11.100\share\MES\Deploys\MesWEB\MesWEB.zip"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "\\192.168.11.100\share\MES\Deploys\MesWEB\Backup\MesWEB_$timestamp.zip"

Write-Host "=== MesWEB 発行パッケージ作成 ===" -ForegroundColor Green

# 既存のzipファイルをバックアップ
if (Test-Path $zipDestination) {
    Write-Host "既存のzipファイルをバックアップしています..." -ForegroundColor Yellow
    $backupDir = Split-Path $backupPath -Parent
    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }
    Copy-Item $zipDestination $backupPath -Force
    Write-Host "バックアップ完了: $backupPath" -ForegroundColor Green
}

# 発行フォルダが存在するか確認
if (-not (Test-Path $publishPath)) {
    Write-Host "エラー: 発行フォルダが見つかりません: $publishPath" -ForegroundColor Red
    Write-Host "先に発行を実行してください (Ctrl+Shift+P -> Publish)" -ForegroundColor Yellow
    exit 1
}

# Zipファイルを作成
Write-Host "Zipファイルを作成しています..." -ForegroundColor Yellow
try {
    Compress-Archive -Path "$publishPath\*" -DestinationPath $zipDestination -Force
    Write-Host "Zipファイル作成完了: $zipDestination" -ForegroundColor Green
    
    # ファイルサイズを表示
    $fileSize = (Get-Item $zipDestination).Length / 1MB
    Write-Host ("ファイルサイズ: {0:N2} MB" -f $fileSize) -ForegroundColor Cyan
}
catch {
    Write-Host "エラー: Zipファイルの作成に失敗しました" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`n=== 完了 ===" -ForegroundColor Green
Write-Host "発行パッケージ: $zipDestination"
