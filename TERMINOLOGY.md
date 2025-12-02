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
| **ラベル** | テンプレートをグループ化するカテゴリ | `CellMappingLabels` |
| **保存** | 新規テンプレートを作成 | 新規INSERT |
| **上書き** | 既存テンプレートを更新 | UPDATE（全項目を削除して再作成） |
| **読み込み** | テンプレートから設定を復元 | SELECT + マッピング |
| **削除** | テンプレートを削除 | DELETE（CASCADE） |
| **マッピング項目** | テンプレート内の個別のセル対応 | `CellMappingItems` |
| **ツリー表示** | ラベルとテンプレートの階層構造表示 | アコーディオン形式 |

### 2.7 インデックス管理

| 用語 | 説明 | プロパティ名 | 補足 |
|------|------|-------------|------|
| **ブックインデックス** | `uploadedBooks` 配列内のブック位置 | `Book1Index`, `Book2Index` | 0始まり、-1=未設定 |
| **ファイル名ベース** | ファイル名で自動的にインデックスを解決 | - | アップロード順序に依存しない |
| **自動マッチング** | ファイル名が一致するブックを自動検索 | `UpdateBookIndexFromFileName()` | 大文字小文字を区別しない |

---

## 3. セルマッピングテンプレート管理

### 3.1 基本概念

| 用語 | 説明 | URL |
|------|------|-----|
| **テンプレート管理ページ** | Excel比較用のテンプレートを管理する画面 | `/cell-mapping/templates` |
| **ラベルツリー** | ラベルとテンプレートの階層構造 | アコーディオン表示 |

### 3.2 ラベル管理

| 用語 | 説明 | 操作 |
|------|------|------|
| **ラベル** | テンプレートをグループ化するカテゴリ | `CellMappingLabel` エンティティ |
| **ラベル名** | ラベルの名称 | 例: 「装置A」「装置B」 |
| **表示順** | ラベルの並び順 | `SortOrder` |
| **ラベル追加** | 新しいラベルを作成 | 入力欄に名前を入力して「追加」 |
| **ラベル編集** | ラベル名を変更 | 「名称編集」ボタン |
| **ラベル削除** | ラベルを削除 | テンプレートが空の場合のみ可能 |

### 3.3 テンプレート操作

| 用語 | 説明 | 操作 |
|------|------|------|
| **テンプレート追加** | ラベル内に新しいテンプレートを作成 | 各ラベルの最下行から追加 |
| **テンプレート編集** | テンプレート名・説明を変更 | 「編集」ボタン |
| **テンプレート削除** | テンプレートを削除 | マッピング項目も一括削除 |
| **テンプレート移動** | 未分類のテンプレートをラベルに移動 | ドロップダウンで選択 |
| **未分類** | ラベルに属さないテンプレート | `LabelId = null` |

---

## 4. 結晶育成ノート（GrowthNote）

### 4.1 データ項目

| 用語 | 説明 | プロパティ名 |
|------|------|-------------|
| **結晶育成日** | 結晶を育成した日付 | `CrystalGrowthDate` |
| **担当者** | 育成を実施した人 | `Operator` |
| **装置名** | 使用した育成装置 | `MachineName` |
| **添加元素** | 添加したドーパント | `Dopants` |
| **添加量** | ドーパントの量 | `DopantAmount` |
| **添加濃度** | ドーパント濃度 | `ppm` |
| **結晶ロット** | 育成した結晶のロット番号 | `CrystalLot` |
| **ペレット投入量** | ペレットの投入量 | `PelletFeed` |
| **ペレットロット** | 使用したペレットのロット番号 | `PelletLot1`, `PelletLot2`, `PelletLot3` |
| **カレット投入量** | カレットの投入量 | `CulletFeed` |
| **カレットロット** | 使用したカレットのロット番号 | `CulletLot1`, `CulletLot2`, `CulletLot3` |
| **坩堝番号** | 使用した坩堝の番号 | `CrucibleName` |
| **坩堝使用回数** | 坩堝の使用回数 | `CrucibleCount` |
| **引上げ時間** | 結晶引上げに要した時間 | `DeltaT` |
| **坩堝内カオウール** | 坩堝内の断熱材構成 | `InsideInsulationComposition` |
| **坩堝底カオウール** | 坩堝底の断熱材構成 | `BottomInsulationComposition` |
| **炉組条件** | 炉の組み立て条件 | `FurnaceCondition1`, `FurnaceCondition2` |
| **リング高さ** | リングの高さ位置 | `RingHeightPosition` |
| **ジルコニア円板** | 使用したジルコニア円板 | `UsingDisk` |
| **育成開始時刻** | 育成を開始した時刻 | `GrowthStartTime` |
| **肩育成終了時刻** | 肩育成が終了した時刻 | `ShoulderEndTime` |
| **ネッキング時電源出力** | ネッキング時の電源出力 | `NeckingVoltage` |
| **育成時電源出力** | 育成時の電源出力 | `GrowthVoltage` |
| **シード位置** | シードの高さ位置 | `SeedHeightPosition` |
| **初期引上げ速度** | 初期の引上げ速度 | `StartPullingSpeed` |
| **肩育成引上げ速度** | 肩育成時の引上げ速度 | `ShoulderEndPullingSpeed` |
| **直胴部引上げ速度** | 直胴部の引上げ速度 | `LastPullingSpeed` |
| **初期シード回転数** | 初期のシード回転数 | `FirstRotationalSpeed` |
| **肩育成シード回転数** | 肩育成時のシード回転数 | `ShoulderEndRotationalSpeed` |
| **直胴部シード回転数** | 直胴部のシード回転数 | `LastRotationalSpeed` |
| **ワークコイル重心位置** | ワークコイルの重心位置 | `GravityCenterWC` |
| **育成時ワークコイル位置** | 育成時のワークコイル位置 | `HeightPositionWC` |
| **結晶ヘッド径** | 結晶ヘッド部の直径 | `HeadDiameter` |
| **結晶テール径** | 結晶テール部の直径 | `TailDiameter` |
| **電源出力電圧** | 電源の出力電圧 | `OutputVoltage` |
| **電源出力電流** | 電源の出力電流 | `OutputCurrent` |
| **液面高さ** | 液面の高さ | `LiquidLevel` |
| **備考** | その他の備考 | `Remarks` |

### 4.2 UI関連

| 用語 | 説明 | 補足 |
|------|------|------|
| **装置名ごと最新表示** | 装置名ごとに最新1件のみ表示 | デフォルト表示モード |
| **全件表示** | すべてのレコードを表示 | 時系列順（降順） |
| **絞込機能** | 複数条件でレコードを絞り込む | 期間、担当者、装置名、添加元素、結晶ロット |
| **横並びテーブル** | 項目を行、レコードを列に表示 | スプレッドシート形式 |
| **Android対応ページ** | モバイル最適化されたページ | `EditGrowthNote.Android.razor` |
| **オートコンプリート** | 過去の入力候補を表示する機能 | Awesomplete ライブラリ |
| **記号挿入** | 特殊記号（±、.、/）を入力する機能 | `InsertSymbol()` |
| **アクセスカウンター** | ページのアクセス回数を記録 | `PageAccessCounters` テーブル |

---

## 5. 共通技術用語

### 5.1 データベース

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **Entity Framework Core** | .NET のORM | `Microsoft.EntityFrameworkCore` |
| **マイグレーション** | データベーススキーマのバージョン管理 | `dotnet ef migrations add` |
| **接続文字列** | データベース接続情報 | `appsettings.json` の `ConnectionStrings` |
| **SQL Server** | Microsoft のリレーショナルデータベース | 本番環境で使用 |
| **AsNoTracking** | 読み取り専用クエリの最適化 | パフォーマンス向上 |
| **CancellationToken** | 非同期処理のタイムアウト制御 | 30秒のタイムアウト設定 |

### 5.2 Blazor コンポーネント

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **@page** | ページのルーティングディレクティブ | `@page "/excel-compare"` |
| **@rendermode** | コンポーネントのレンダリングモード | `@rendermode InteractiveServer` |
| **@bind** | 双方向データバインディング | `@bind="value"` |
| **@bind:after** | バインド後のイベントハンドラ | `@bind:after="OnValueChanged"` |
| **@inject** | 依存性注入 | `@inject ILogger<T> Logger` |
| **StateHasChanged()** | UIの再レンダリングをトリガー | 非同期処理後に呼び出す |
| **IDisposable** | リソース解放のインターフェース | イベントのサブスクライブ解除 |

### 5.3 JavaScript連携

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **JSInterop** | C# と JavaScript 間の相互運用 | `IJSRuntime` |
| **JSInvokable** | JavaScript から C# メソッドを呼び出す属性 | `[JSInvokable]` |
| **DotNetObjectReference** | JavaScript に渡す C# オブジェクトの参照 | `DotNetObjectReference.Create(this)` |
| **Sortable.js** | ドラッグ&ドロップのライブラリ | 項目の並べ替え |
| **Awesomplete** | オートコンプリートライブラリ | 入力補完 |

### 5.4 ファイル処理

| 用語 | 説明 | ライブラリ |
|------|------|-----------|
| **ClosedXML** | .xlsx ファイルの読み書き | `ClosedXML.Excel` |
| **ExcelDataReader** | .xls ファイルの読み込み | `ExcelDataReader` |
| **InputFile** | Blazor のファイルアップロードコンポーネント | `<InputFile>` |
| **StreamReader** | テキストストリームの読み込み | CSV出力 |
| **UTF-8 BOM** | UTF-8エンコーディングのバイトオーダーマーク | CSV のExcel互換性 |

---

## 6. 開発環境・デプロイ

### 6.1 開発環境

| 用語 | 説明 | 備考 |
|------|------|------|
| **.NET 9 / .NET 10** | ターゲットフレームワーク | プロジェクトごとに異なる場合あり |
| **Visual Studio** | 統合開発環境 | 推奨IDE |
| **Podman** | コンテナ実行エンジン | Docker互換 |
| **Docker** | コンテナ実行エンジン | Rancher Desktop経由 |
| **Rancher Desktop** | Dockerデスクトップ代替 | Windows環境 |
| **WSL2** | Windows Subsystem for Linux | Docker実行環境 |

### 6.2 設定ファイル

| 用語 | 説明 | ファイル名 |
|------|------|-----------|
| **appsettings.json** | アプリケーション設定ファイル | 接続文字列・ログレベル |
| **launchSettings.json** | デバッグプロファイル設定 | HTTP/HTTPS ポート設定 |
| **docker-compose.yml** | Docker コンテナ構成定義 | マルチコンテナ環境 |
| **Dockerfile** | イメージのビルド手順 | `Dockerfile.simple` |

### 6.3 Docker/Podman関連

| 用語 | 説明 | 使用例 |
|------|------|--------|
| **コンテナ** | アプリケーションの実行環境 | `podman ps` |
| **イメージ** | コンテナの設計図 | `podman images` |
| **ボリューム** | データ永続化のストレージ | `sqlserver_data` |
| **ポートバインディング** | コンテナポートをホストに公開 | `8080:8080` |
| **ホストネットワーク** | ホストのネットワークを直接使用 | `--network host` |

---

## 7. 命名規則

### 7.1 変数・プロパティ

| パターン | 説明 | 例 |
|---------|------|-----|
| **camelCase** | ローカル変数・プライベートフィールド | `cellMappings`, `uploadedBooks` |
| **PascalCase** | プロパティ・メソッド・クラス | `ItemName`, `CompareFiles()`, `CellMapping` |
| **_camelCase** | プライベートフィールド（オプション） | `_logger` |

### 7.2 ファイル・フォルダ

| パターン | 説明 | 例 |
|---------|------|-----|
| **PascalCase** | C# ファイル・クラス | `ExcelCompare.razor.cs` |
| **kebab-case** | URL・ルート | `/excel-compare` |
| **Components/Pages/** | Blazor ページコンポーネント | `ExcelCompare.razor` |
| **Components/Layout/** | レイアウトコンポーネント | `MainLayout.razor` |
| **Data/** | データモデル・DbContext | `AppDbContext.cs` |
| **Migrations/** | EF Core マイグレーション | `20250902090304_InitialCreate.cs` |

---

## 8. よくある誤解・注意事項

### 8.1 ExcelCompare

| 誤用例 | 正しい用語 | 説明 |
|--------|-----------|------|
| 「ファイル1」 | 「ブック1」 | Excel用語に統一 |
| 「セルマッピング」 | 「セル対応表」 | UI表示に合わせる |
| 「ブックのインデックス」 | 「ブックインデックス」 | プロパティ名に準拠 |
| 「テンプレートの編集」 | 「テンプレートの上書き」 | 動作を正確に表現 |

### 8.2 一括変換

| 誤解 | 実際の動作 |
|------|-----------|
| 「一括変換は最初の項目だけ変換する」 | **すべての一致する項目を変換**（モード選択に注意） |
| 「変換先はアップロード済みファイルから必ず選ぶ」 | **ファイル名は手入力も可能（インデックスは未設定）** |
| 「変換モードは毎回リセットされる」 | **前回の選択が保持される**（モーダルを開き直すとリセット） |

### 8.3 テンプレート

| 誤解 | 実際の動作 |
|------|-----------|
| 「テンプレートを読み込むとブックもアップロードされる」 | **ファイル名のみ保存され、自動マッチング** |
| 「上書き保存は部分更新」 | **全項目を削除して再作成（完全置換）** |
| 「テンプレート名が空だと新規保存になる」 | **選択中のテンプレートがある場合は上書き** |

### 8.4 育成ノート

| 誤解 | 実際の動作 |
|------|-----------|
| 「絞込中は最新表示モードが有効」 | **絞込中は常に全件表示**（絞込解除後にモード復帰） |
| 「装置名ごと最新表示は全装置が表示される」 | **装置名が空のレコードは除外される** |
| 「削除は確認なし」 | **JavaScriptの確認ダイアログが表示される** |

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
| **SA** | System Administrator | SQL Serverの管理者アカウント |

---

## 10. バージョン情報

| 項目 | バージョン | 備考 |
|------|-----------|------|
| ドキュメント作成日 | 2025-01-10 | 初版 |
| ドキュメント更新日 | 2025-01-XX | セルマッピングテンプレート管理追加 |
| 対象アプリ | MesWEB v1.0 | - |
| .NET バージョン | .NET 9 / .NET 10 | プロジェクトにより異なる |
| Blazor | Blazor Server | - |

---

## 更新履歴

| 日付 | 変更内容 | 担当者 |
|------|---------|--------|
| 2025-01-10 | 初版作成 | - |
| 2025-01-XX | セルマッピングテンプレート管理機能の用語追加 | - |
| 2025-01-XX | 育成ノート絞込機能の用語追加 | - |

---

## 参考資料

- [Microsoft Blazor ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Entity Framework Core ドキュメント](https://learn.microsoft.com/ja-jp/ef/core/)
- [ClosedXML GitHub](https://github.com/ClosedXML/ClosedXML)
- [ExcelDataReader GitHub](https://github.com/ExcelDataReader/ExcelDataReader)
- [Sortable.js GitHub](https://github.com/SortableJS/Sortable)
- [Awesomplete GitHub](https://github.com/LeaVerou/awesomplete)

---

**このドキュメントは、チーム全体の認識統一と円滑なコミュニケーションのために定期的に更新してください。**
