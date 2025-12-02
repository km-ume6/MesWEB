# MesWEB アプリケーション用語集

このドキュメントは、MesWEBアプリケーション開発における技術用語と概念を統一し、チーム内のコミュニケーションを円滑にするためのリファレンスです。

---

## 1. アプリケーション全体

### 1.1 プロジェクト構成

| 用語 | 説明 | 備考 |
|------|------|------|
| **MesWEB** | メインプロジェクト（Blazor Server アプリケーション） | ホストプロジェクト |
| **MesWEB.Shared** | 共通ライブラリ（データモデル、サービス、DbContext） | 全プロジェクトから参照 |
| **MesWEB.ExcelCompare** | Excel比較機能（Razor Class Library） | 機能別RCL |
| **MesWEB.GrowthNote** | 結晶育成ノート機能（Razor Class Library） | 機能別RCL |

### 1.2 アーキテクチャ用語

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **Blazor Server** | サーバーサイドで動作するBlazorアプリケーション | アプリケーションの実行モデル |
| **RCL (Razor Class Library)** | 再利用可能なRazorコンポーネントライブラリ | 機能別モジュール化 |
| **Interactive Server** | サーバー側で対話的に実行されるレンダリングモード | `@rendermode InteractiveServer` |
| **DbContext** | Entity Framework Core のデータベースコンテキスト | `AppDbContext` |
| **DbContextFactory** | DbContext インスタンスを生成するファクトリー | Blazor Server の並行処理対応 |

---

## 2. Excel比較機能（ExcelCompare）

### 2.1 基本概念

| 用語 | 説明 | 補足 |
|------|------|------|
| **セル対応表** | 比較するセルのマッピング情報を管理するテーブル | `cellMappings` |
| **マッピング** | 2つのファイル・シート・セル間の対応関係 | `CellMapping` クラス |
| **項目** | セル対応表の1行（1つのマッピング） | 「項目を追加」「項目を削除」 |
| **テンプレート** | セル対応表の設定を保存したもの | データベースに永続化 |

### 2.2 ファイル関連用語

| 用語 | 英語表記 | 説明 | プロパティ名 |
|------|----------|------|-------------|
| **ブック** | Book | Excelファイル全体 | `Book1FileName`, `Book2FileName` |
| **ファイル名** | FileName | ブックのファイル名（拡張子含む） | `FileName` |
| **アップロード済みブック** | Uploaded Book | メモリ上に読み込まれたExcelファイル | `uploadedBooks` |
| **シート** | Sheet | ブック内のワークシート | `Sheet1Name`, `Sheet2Name` |
| **セル** | Cell | シート内の単一セル | `Sheet1Cell`, `Sheet2Cell` |
| **セルアドレス** | Cell Address | セルの位置（例: A1, B5） | 単一セルまたは範囲 |
| **範囲** | Range | 複数セルの範囲（例: A1:A10） | `:` を含む表記 |

### 2.3 比較・計算関連用語

| 用語 | 英語表記 | 説明 | 列挙型 |
|------|----------|------|--------|
| **数式** | Formula | セル範囲に適用する計算式 | `FormulaType` |
| **なし** | None | セル値をそのまま使用 | `FormulaType.None` |
| **最大値** | Max | 範囲内の最大値 | `FormulaType.Max` |
| **最小値** | Min | 範囲内の最小値 | `FormulaType.Min` |
| **平均値** | Average | 範囲内の平均値 | `FormulaType.Average` |
| **小数点以下桁数** | Decimal Places | 数値の丸め精度 | `DecimalPlaces` (0～10) |
| **許容誤差** | Tolerance | 数値比較時の許容範囲（±） | `Tolerance` |
| **比較結果** | Comparison Result | セル値の比較結果 | `CellComparisonResult` |
| **一致** | Match | 2つの値が一致 | `IsMatch = true` |
| **不一致** | Mismatch | 2つの値が不一致 | `IsMatch = false` |

### 2.4 UI操作用語

| 用語 | 説明 | 対応する操作 |
|------|------|-------------|
| **展開** | 項目の詳細フォームを表示する | `ExpandAll()` |
| **折りたたむ** | 項目の詳細フォームを非表示にする | `CollapseAll()` |
| **ドラッグハンドル** | 項目をドラッグして並べ替えるアイコン（≡） | Sortable.js |
| **コンパクト表示** | 項目を1行で要約表示するモード | デフォルト状態 |
| **詳細表示** | 項目のすべてのフィールドを表示するモード | 展開状態 |
| **フローティングツールバー** | 画面右下に固定表示されるボタン群 | スクロール・展開・追加 |

### 2.5 一括変換機能

| 用語 | 説明 | 備考 |
|------|------|------|
| **一括変換** | 複数の項目のファイル名・シート名を一括で置き換える機能 | Bulk Rename |
| **変換モード** | 一括変換の対象範囲 | `RenameMode` enum |
| **ブック1のファイル名** | Book1のファイル名のみ変換 | `RenameMode.Book1` |
| **ブック2のファイル名** | Book2のファイル名のみ変換 | `RenameMode.Book2` |
| **両方のファイル名** | Book1とBook2の両方を変換 | `RenameMode.BothBooks` |
| **ブック1のシート名** | Book1のシート名のみ変換 | `RenameMode.Sheet1` |
| **ブック2のシート名** | Book2のシート名のみ変換 | `RenameMode.Sheet2` |
| **変換元** | 置き換え前の名前 | `sourceBookName`, `sourceSheetName` |
| **変換先** | 置き換え後の名前 | `targetBookName`, `targetSheetName` |

### 2.6 テンプレート管理

| 用語 | 説明 | データベーステーブル |
|------|------|---------------------|
| **テンプレート** | セル対応表の保存された設定 | `CellMappingTemplates` |
| **テンプレート名** | テンプレートの名称 | `TemplateName` |
| **説明** | テンプレートの補足情報 | `Description` |
| **保存** | 新規テンプレートを作成 | 新規INSERT |
| **上書き** | 既存テンプレートを更新 | UPDATE（全項目を削除して再作成） |
| **読み込み** | テンプレートから設定を復元 | SELECT + マッピング |
| **削除** | テンプレートを削除 | DELETE（CASCADE） |
| **マッピング項目** | テンプレート内の個別のセル対応 | `CellMappingItems` |

### 2.7 インデックス管理

| 用語 | 説明 | プロパティ名 | 補足 |
|------|------|-------------|------|
| **ブックインデックス** | `uploadedBooks` 配列内のブック位置 | `Book1Index`, `Book2Index` | 0始まり、-1=未設定 |
| **ファイル名ベース** | ファイル名で自動的にインデックスを解決 | - | アップロード順序に依存しない |
| **自動マッチング** | ファイル名が一致するブックを自動検索 | `UpdateBookIndexFromFileName()` | 大文字小文字を区別しない |

---

## 3. 結晶育成ノート（GrowthNote）

### 3.1 データ項目

| 用語 | 説明 | プロパティ名 |
|------|------|-------------|
| **結晶育成日** | 結晶を育成した日付 | `CrystalGrowthDate` |
| **担当者** | 育成を実施した人 | `Operator` |
| **装置名** | 使用した育成装置 | `MachineName` |
| **添加元素** | 添加したドーパント | `Dopants` |
| **添加量** | ドーパントの量 | `DopantAmount` |
| **結晶ロット** | 育成した結晶のロット番号 | `CrystalLot` |
| **ペレットロット** | 使用したペレットのロット番号 | `PelletLot1`, `PelletLot2`, `PelletLot3` |
| **カレットロット** | 使用したカレットのロット番号 | `CulletLot1`, `CulletLot2`, `CulletLot3` |
| **坩堝番号** | 使用した坩堝の番号 | `CrucibleName` |
| **坩堝使用回数** | 坩堝の使用回数 | `CrucibleCount` |
| **炉組条件** | 炉の組み立て条件 | `FurnaceCondition1`, `FurnaceCondition2` |
| **リング高さ** | リングの高さ位置 | `RingHeightPosition` |
| **ワークコイル重心位置** | ワークコイルの重心位置 | `GravityCenterWC` |
| **育成時ワークコイル位置** | 育成時のワークコイル位置 | `HeightPositionWC` |

### 3.2 UI関連

| 用語 | 説明 | 補足 |
|------|------|------|
| **Android対応ページ** | モバイル最適化されたページ | `EditGrowthNote.Android.razor` |
| **オートコンプリート** | 過去の入力候補を表示する機能 | Awesomplete ライブラリ |
| **記号挿入** | 特殊記号（±、.、/）を入力する機能 | `InsertSymbol()` |

---

## 4. 共通技術用語

### 4.1 データベース

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **Entity Framework Core** | .NET のORM | `Microsoft.EntityFrameworkCore` |
| **マイグレーション** | データベーススキーマのバージョン管理 | `dotnet ef migrations add` |
| **接続文字列** | データベース接続情報 | `appsettings.json` の `ConnectionStrings` |
| **SQL Server** | Microsoft のリレーショナルデータベース | 本番環境で使用 |

### 4.2 Blazor コンポーネント

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **@page** | ページのルーティングディレクティブ | `@page "/excel-compare"` |
| **@rendermode** | コンポーネントのレンダリングモード | `@rendermode InteractiveServer` |
| **@bind** | 双方向データバインディング | `@bind="value"` |
| **@bind:after** | バインド後のイベントハンドラ | `@bind:after="OnValueChanged"` |
| **@inject** | 依存性注入 | `@inject ILogger<T> Logger` |
| **StateHasChanged()** | UIの再レンダリングをトリガー | 非同期処理後に呼び出す |

### 4.3 JavaScript連携

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **JSInterop** | C# と JavaScript 間の相互運用 | `IJSRuntime` |
| **JSInvokable** | JavaScript から C# メソッドを呼び出す属性 | `[JSInvokable]` |
| **DotNetObjectReference** | JavaScript に渡す C# オブジェクトの参照 | `DotNetObjectReference.Create(this)` |
| **Sortable.js** | ドラッグ&ドロップのライブラリ | 項目の並べ替え |

### 4.4 ファイル処理

| 用語 | 説明 | ライブラリ |
|------|------|-----------|
| **ClosedXML** | .xlsx ファイルの読み書き | `ClosedXML.Excel` |
| **ExcelDataReader** | .xls ファイルの読み込み | `ExcelDataReader` |
| **InputFile** | Blazor のファイルアップロードコンポーネント | `<InputFile>` |
| **StreamReader** | テキストストリームの読み込み | CSV出力 |
| **UTF-8 BOM** | UTF-8エンコーディングのバイトオーダーマーク | CSV のExcel互換性 |

---

## 5. 開発環境・デプロイ

### 5.1 開発環境

| 用語 | 説明 | 備考 |
|------|------|------|
| **.NET 9 / .NET 10** | ターゲットフレームワーク | プロジェクトごとに異なる場合あり |
| **Visual Studio** | 統合開発環境 | 推奨IDE |
| **Rancher Desktop** | Dockerデスクトップ代替 | Windows環境 |
| **WSL2** | Windows Subsystem for Linux | Docker実行環境 |

### 5.2 設定ファイル

| 用語 | 説明 | ファイル名 |
|------|------|-----------|
| **appsettings.json** | アプリケーション設定ファイル | 接続文字列・ログレベル |
| **launchSettings.json** | デバッグプロファイル設定 | HTTP/HTTPS ポート設定 |
| **docker-compose.yml** | Docker コンテナ構成定義 | マルチコンテナ環境 |

### 5.3 Docker関連

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **コンテナ** | アプリケーションの実行環境 | `docker ps` |
| **イメージ** | コンテナの設計図 | `docker images` |
| **Dockerfile** | イメージのビルド手順 | `Dockerfile.simple` |
| **Docker Compose** | 複数コンテナの管理ツール | `docker-compose up` |
| **ボリューム** | データ永続化のストレージ | `sqlserver_data` |

---

## 6. 命名規則

### 6.1 変数・プロパティ

| パターン | 説明 | 例 |
|---------|------|-----|
| **camelCase** | ローカル変数・プライベートフィールド | `cellMappings`, `uploadedBooks` |
| **PascalCase** | プロパティ・メソッド・クラス | `ItemName`, `CompareFiles()`, `CellMapping` |
| **_camelCase** | プライベートフィールド（オプション） | `_logger` |

### 6.2 ファイル・フォルダ

| パターン | 説明 | 例 |
|---------|------|-----|
| **PascalCase** | C# ファイル・クラス | `ExcelCompare.razor.cs` |
| **kebab-case** | URL・ルート | `/excel-compare` |
| **Components/Pages/** | Blazor ページコンポーネント | `ExcelCompare.razor` |
| **Components/Layout/** | レイアウトコンポーネント | `MainLayout.razor` |
| **Data/** | データモデル・DbContext | `AppDbContext.cs` |
| **Migrations/** | EF Core マイグレーション | `20250902090304_InitialCreate.cs` |

---

## 7. よくある誤解・注意事項

### 7.1 ExcelCompare

| 誤用例 | 正しい用語 | 説明 |
|--------|-----------|------|
| 「ファイル1」 | 「ブック1」 | Excel用語に統一 |
| 「セルマッピング」 | 「セル対応表」 | UI表示に合わせる |
| 「ブックのインデックス」 | 「ブックインデックス」 | プロパティ名に準拠 |
| 「テンプレートの編集」 | 「テンプレートの上書き」 | 動作を正確に表現 |

### 7.2 一括変換

| 誤解 | 実際の動作 |
|------|-----------|
| 「一括変換は最初の項目だけ変換する」 | **すべての一致する項目を変換**（モード選択に注意） |
| 「変換先はアップロード済みファイルから必ず選ぶ」 | **ファイル名は手入力も可能（インデックスは未設定）** |
| 「変換モードは毎回リセットされる」 | **前回の選択が保持される**（モーダルを開き直すとリセット） |

### 7.3 テンプレート

| 誤解 | 実際の動作 |
|------|-----------|
| 「テンプレートを読み込むとブックもアップロードされる」 | **ファイル名のみ保存され、自動マッチング** |
| 「上書き保存は部分更新」 | **全項目を削除して再作成（完全置換）** |
| 「テンプレート名が空だと新規保存になる」 | **選択中のテンプレートがある場合は上書き** |

---

## 8. コミュニケーション例

### 8.1 良い例

- 「Excel比較のセル対応表で、ブック1のファイル名を一括変換したい」
- 「テンプレートを上書き保存するとき、既存の項目はすべて削除されますか？」
- 「一括変換モードを『BothBooks』にすると、Book1とBook2の両方が変わる」

### 8.2 改善が必要な例

- 「ファイル1の名前を変える」 → 「ブック1のファイル名を変更する」
- 「セルのマッピングを編集」 → 「セル対応表の項目を編集する」
- 「テンプレートの更新」 → 「テンプレートの上書き保存」

---

## 9. 略語・頭字語

| 略語 | 正式名称 | 説明 |
|------|---------|------|
| **RCL** | Razor Class Library | 再利用可能なコンポーネントライブラリ |
| **EF Core** | Entity Framework Core | ORM フレームワーク |
| **JSInterop** | JavaScript Interoperability | JavaScript連携機能 |
| **DP** | Decimal Places | 小数点以下桁数 |
| **WSL** | Windows Subsystem for Linux | Linux互換レイヤー |
| **CSV** | Comma-Separated Values | カンマ区切りファイル |
| **BOM** | Byte Order Mark | エンコーディングマーカー |

---

## 10. バージョン情報

| 項目 | バージョン | 備考 |
|------|-----------|------|
| ドキュメント作成日 | 2025-01-10 | 初版 |
| 対象アプリ | MesWEB v1.0 | - |
| .NET バージョン | .NET 9 / .NET 10 | プロジェクトにより異なる |
| Blazor | Blazor Server | - |

---

## 更新履歴

| 日付 | 変更内容 | 担当者 |
|------|---------|--------|
| 2025-01-10 | 初版作成 | - |

---

## 参考資料

- [Microsoft Blazor ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Entity Framework Core ドキュメント](https://learn.microsoft.com/ja-jp/ef/core/)
- [ClosedXML GitHub](https://github.com/ClosedXML/ClosedXML)
- [ExcelDataReader GitHub](https://github.com/ExcelDataReader/ExcelDataReader)

---

**このドキュメントは、チーム全体の認識統一と円滑なコミュニケーションのために定期的に更新してください。**
