# MesWEB プロジェクト

## プロジェクト構成

```
MesWEB/
├── MesWEB/                      # メインWebアプリケーション (Blazor Server)
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Home.razor      # ホームページ（唯一のルート "/"）
│   │   └── Layout/
│   │       └── MainLayout.razor # メインレイアウト
│   ├── Program.cs              # エントリーポイント
│   └── appsettings.json        # 設定ファイル（メインプロジェクトのみ）
│
├── MesWEB.ExcelCompare/         # Excel比較機能 (RCL)
│   └── Components/Pages/
│       └── ExcelCompare.razor  # ルート: /excel-compare
│
├── MesWEB.GrowthNote/           # 育成ノート機能 (RCL)
│   └── Components/Pages/
│       └── GrowthNote.razor    # ルート: /growth-note
│
└── MesWEB.Shared/               # 共有ライブラリ
    ├── Data/
    │   ├── AppDbContext.cs     # Entity Framework DbContext
    │   ├── GrowthNoteItem.cs
    │   └── CellMappingTemplate.cs
    └── Services/
        └── DeviceDetectionService.cs
```

## 技術スタック

- **フレームワーク**: .NET 10.0 (メインプロジェクト), .NET 9.0 (RCL)
- **UI**: Blazor Server (Interactive Server)
- **データベース**: SQL Server (Entity Framework Core 9.0.10)
- **Excel処理**: ClosedXML, ExcelDataReader

## 重要な設計上の決定

### 1. ルート "/"の所有権
- ✅ **メインプロジェクトの`Home.razor`のみ**が`@page "/"`を持つ
- ❌ RCLプロジェクトには`Home.razor`や`Counter.razor`を含めない

### 2. 名前空間の統一
- `AppDbContext`: `MesWEB.Shared.Data`
- `DeviceDetectionService`: `MesWEB.Shared.Services`
- `Program.cs`で正しい名前空間を参照

### 3. appsettings.jsonの管理
- ✅ メインプロジェクト(`MesWEB`)のみに配置
- ❌ RCLプロジェクトには配置しない（発行時の競合を防ぐ）

## 開発

### デバッグ実行
```bash
cd MesWEB
dotnet run
```
アクセス: `http://localhost:5000`

### 発行
Visual Studioで:
1. `MesWEB`プロジェクトを右クリック → 「発行」
2. `FolderProfile`を選択
3. 「発行」ボタンをクリック

出力先: `\\192.168.11.100\share\MES\Deploys\MesWEB\Deploy\`

## デプロイ (IIS)

### 前提条件
- ASP.NET Core Hosting Bundle (.NET 10) がインストールされていること
- アプリケーションプール: 「マネージ コードなし」

### 手順
1. 発行後、IISマネージャーで物理パスを設定:
   ```
   \\192.168.11.100\share\MES\Deploys\MesWEB\Deploy
   ```
2. アプリケーションプール: `.NET v4.5` → **「マネージ コードなし」**に変更
3. アプリケーションを開始

### アクセスURL
```
http://サーバー名/MesWEB
```

## データベース

### 接続文字列
`appsettings.json`で設定:
```json
"ConnectionStrings": {
  "Default": "Server=192.168.11.15,1433;Database=CGNotes;..."
}
```

### マイグレーション
```bash
# マイグレーション作成
dotnet ef migrations add MigrationName --project MesWEB.Shared --startup-project MesWEB

# データベース更新
dotnet ef database update --project MesWEB.Shared --startup-project MesWEB
```

## トラブルシューティング

### 問題: ルートの重複エラー
**原因**: RCLプロジェクトに`Home.razor`または`Counter.razor`が存在
**解決**: 削除済み

### 問題: DeviceDetectionService が見つからない
**原因**: 名前空間の不一致
**解決**: `Program.cs`で`using MesWEB.Shared.Services;`を使用

### 問題: HTTP 500.19 (IIS)
**原因**: `web.config`に無効な設定
**解決**: `<httpRuntime>`要素を削除（ASP.NET Core非対応）

## 今後の改善案

1. ✅ デプロイをシンプルなフォルダ発行に統一（完了）
2. ⚠️ RCLの必要性を再評価（機能分離が本当に必要か？）
3. ⚠️ .NET 10と.NET 9の混在を統一
4. ✅ CI/CDパイプラインの検討（GitHub Actions等）

## 参考リンク

- [Blazor Server Documentation](https://learn.microsoft.com/aspnet/core/blazor/hosting-models)
- [Razor Class Libraries](https://learn.microsoft.com/aspnet/core/razor-pages/ui-class)
- [ASP.NET Core Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/)
