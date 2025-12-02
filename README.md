# MesWEB プロジェクト

## プロジェクト概要

MesWEBは、生産技術部門のDX推進を支援するBlazor Serverアプリケーションです。Excel比較機能や結晶育成ノート機能など、業務効率化のための複数の機能を提供します。

## プロジェクト構成

```
MesWEB/
├── MesWEB/                      # メインWebアプリケーション (Blazor Server)
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Home.razor                    # ホームページ（唯一のルート "/"）
│   │   │   └── CellMappingTemplates.razor    # セルマッピングテンプレート管理（/cell-mapping/templates）
│   │   └── Layout/
│   │       └── MainLayout.razor              # メインレイアウト
│   ├── Program.cs              # エントリーポイント
│   ├── appsettings.json        # 設定ファイル（メインプロジェクトのみ）
│   └── CONTAINER_README.md     # コンテナ実行ガイド
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
    │   ├── AppDbContext.cs                    # Entity Framework DbContext
    │   ├── GrowthNoteItem.cs                  # 育成ノートエンティティ
    │   ├── CellMappingTemplate.cs             # セルマッピングテンプレートエンティティ
    │   └── PageAccessCounter.cs               # アクセスカウンターエンティティ
    └── Services/
        └── DeviceDetectionService.cs          # デバイス検出サービス
```

## 主要機能

### 1. ホーム（/）
- 生産技術部門のDXビジョンを表示
- 各機能へのナビゲーション

### 2. Excel比較（/excel-compare）
- 2つのExcelファイルを比較
- セル対応表によるカスタマイズ可能な比較
- テンプレート機能で比較設定を保存・再利用
- 一括変換機能でファイル名・シート名を効率的に変更
- 比較結果のCSV出力

### 3. セルマッピングテンプレート管理（/cell-mapping/templates）
- Excel比較用のテンプレートを管理
- ラベル（カテゴリ）によるテンプレートの分類
- ツリー構造での階層的な管理
- テンプレートの作成・編集・削除

### 4. 育成ノート（/growth-note）
- 結晶育成の記録を管理
- 装置名ごとの最新表示/全件表示の切り替え
- 多様な絞込条件（期間、担当者、装置名、添加元素、結晶ロット）
- PC/Android対応の入力画面
- アクセスカウンター機能

## 技術スタック

- **フレームワーク**: .NET 10.0 (メインプロジェクト), .NET 9.0 (RCL)
- **UI**: Blazor Server (Interactive Server)
- **データベース**: SQL Server (Entity Framework Core 9.0.10)
- **Excel処理**: ClosedXML, ExcelDataReader
- **JavaScript連携**: Sortable.js, Awesomplete

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

### 4. レンダリングモード
- 全ページで `@rendermode InteractiveServer` を使用
- サーバー側での対話的な処理を実現

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

## コンテナ実行

コンテナでの実行については、`MesWEB/CONTAINER_README.md` を参照してください。

- Podman/Dockerでの実行
- SQL Serverコンテナとの連携
- LAN内からのアクセス設定

## データベース

### 接続文字列
`appsettings.json`で設定:
```json
{
  "ConnectionStrings": {
    "Default": "Server=192.168.11.15,1433;Database=CGNotes;User Id=sa;Password=***;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

### マイグレーション
```bash
# マイグレーション作成
dotnet ef migrations add MigrationName --project MesWEB.Shared --startup-project MesWEB

# データベース更新
dotnet ef database update --project MesWEB.Shared --startup-project MesWEB
```

### 主要なテーブル
- `GrowthNotes`: 育成ノートの記録
- `CellMappingTemplates`: Excel比較テンプレート
- `CellMappingItems`: テンプレートの詳細項目
- `CellMappingLabels`: テンプレートのカテゴリ
- `PageAccessCounters`: ページアクセスカウンター

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

### 問題: データベース接続エラー
**原因**: 接続文字列の誤りまたはSQL Serverが起動していない
**解決**: 
1. `appsettings.json`の接続文字列を確認
2. SQL Serverの起動状態を確認
3. ファイアウォール設定を確認

### 問題: Excelファイルのアップロードエラー
**原因**: ファイルサイズ制限またはメモリ不足
**解決**:
1. `Program.cs`で`MaxRequestBodySize`を調整
2. サーバーのメモリを確認

## ドキュメント

### 📚 主要ドキュメント

- [README.md](README.md) - プロジェクト概要（本ドキュメント）
- [TERMINOLOGY.md](TERMINOLOGY.md) - アプリケーション用語集

### 📖 ユーザーガイド

- [Excel比較機能](docs/ExcelCompare-UserGuide.md) - Excel比較機能の使い方
- [セルマッピングテンプレート管理](docs/CellMappingTemplates-UserGuide.md) - テンプレート管理の使い方
- [育成ノート](docs/GrowthNote-UserGuide.md) - 育成ノート機能の使い方

### 🐳 コンテナ・デプロイ

- [CONTAINER_README.md](MesWEB/CONTAINER_README.md) - Podman/Dockerでのコンテナ実行ガイド
- [DOCKER_SETUP.md](MesWEB/DOCKER_SETUP.md) - Rancher Desktopのセットアップ
- [RANCHER_DESKTOP_SETUP.md](MesWEB/RANCHER_DESKTOP_SETUP.md) - Rancher Desktop簡易セットアップ
- [DOCKER_COMPOSE_SETUP.md](MesWEB/DOCKER_COMPOSE_SETUP.md) - Docker Composeでの実行（SQL Server含む）

## 今後の改善案

1. ✅ デプロイをシンプルなフォルダ発行に統一（完了）
2. ✅ セルマッピングテンプレート管理機能の追加（完了）
3. ⚠️ .NET 10と.NET 9の混在を統一
4. ⚠️ CI/CDパイプラインの検討（GitHub Actions等）
5. ⚠️ ユニットテストの追加
6. ⚠️ パフォーマンスの最適化（大量データ処理）

## 参考リンク

- [Blazor Server Documentation](https://learn.microsoft.com/aspnet/core/blazor/hosting-models)
- [Razor Class Libraries](https://learn.microsoft.com/aspnet/core/razor-pages/ui-class)
- [ASP.NET Core Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

## ライセンス

社内プロジェクトのため、ライセンスは適用されません。

## 連絡先

生産技術グループ
