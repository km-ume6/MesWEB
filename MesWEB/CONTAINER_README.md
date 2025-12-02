# Crystal Growth Notebook 2 - コンテナ実行ガイド

## 前提条件
- Podmanまたはdocker がインストールされていること
- .NET 9 SDKがインストールされていること（ローカルビルド用）
- **SQL Serverが利用可能であること**

## データベース設定

このアプリケーションは **SQL Server** を使用します。

### 設定ファイル

接続文字列は **`appsettings.json`** の1ファイルのみで管理します。

### 初回セットアップ

1. **テンプレートをコピー**

```bash
cd CrystalGrowthNotebook2
cp appsettings.json.template appsettings.json
```

Windows (PowerShell):
```powershell
cd CrystalGrowthNotebook2
Copy-Item appsettings.json.template appsettings.json
```

2. **接続情報を編集**

`appsettings.json` を開いて、SQL Server接続情報を設定：

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER,1433;Database=CGNotes;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

**?? 重要**:
- このファイルには機密情報が含まれるため、Gitにコミットしないでください
- `.gitignore.custom` に追加済み
- データベース `CGNotes` を事前に作成しておく必要があります

## SQL Serverデータベースの準備

コンテナ起動前に、SQL Serverにデータベースを作成してください：

```sql
CREATE DATABASE CGNotes;
```

マイグレーションはアプリケーション起動時に自動的に実行されます。

## 実行方法

### Podmanで実行（推奨）

#### Windows
```cmd
cd CrystalGrowthNotebook2
.\run-podman.bat
```

#### Linux/Mac
```bash
cd CrystalGrowthNotebook2
chmod +x run-podman.sh
./run-podman.sh
```

### Docker Composeで実行

SQL Serverコンテナも含めて一括起動：

```bash
docker-compose up -d
```

または

```bash
podman-compose up -d
```

## アクセス

### ローカルからアクセス

ブラウザで以下のURLを開きます：
```
http://localhost:8080
```

### LAN内の他のPCからアクセス

#### 1. Windowsファイアウォールの設定

**方法A: 自動設定（推奨）**

`setup-firewall.bat` を**管理者として実行**してください：

1. `setup-firewall.bat` を右クリック
2. **「管理者として実行」**を選択

**方法B: 手動設定**

PowerShellを管理者として開き、以下を実行：

```powershell
New-NetFirewallRule -DisplayName "Crystal Growth Notebook (Podman)" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow -Profile Private,Domain
```

または、Windows Defender ファイアウォールのGUIで設定：
1. Windows キー → 「ファイアウォール」で検索
2. 「詳細設定」→「受信の規則」→「新しい規則」
3. ポート: TCP 8080 を許可

#### 2. このPCのIPアドレスを確認

PowerShellで以下を実行：

```powershell
ipconfig
```

または

```powershell
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*"}
```

例: `192.168.11.23`

#### 3. 他のPCからアクセス

他のPCのブラウザで以下のURLを開きます：

```
http://192.168.11.23:8080
```

（IPアドレスは実際の値に置き換えてください）

## 開発モード

ローカルで開発する場合：

```bash
cd CrystalGrowthNotebook2
dotnet run
```

同じ `appsettings.json` が使用されます。

## 便利なコマンド

### ログ確認
```bash
podman logs -f crystalgrowthnotebook2
```

### コンテナ停止
```bash
podman stop crystalgrowthnotebook2
```

### コンテナ再起動
```bash
podman start crystalgrowthnotebook2
```

### コンテナ削除
```bash
podman rm -f crystalgrowthnotebook2
```

### このPCのIPアドレス確認
```bash
ipconfig
# または
hostname -I  # Linux/Mac
```

## トラブルシューティング

### 接続文字列エラー
- `appsettings.json` の設定を確認
- SQL Server接続情報が正しいか確認
- ファイルが存在しない場合は `appsettings.json.template` からコピー

### SQL Server接続エラー
1. SQL Serverが起動しているか確認
2. ファイアウォールでポート1433が開いているか確認
3. SQL Server認証が有効になっているか確認
4. データベース `CGNotes` が存在するか確認

```sql
-- データベースの存在確認
SELECT name FROM sys.databases WHERE name = 'CGNotes';

-- 存在しない場合は作成
CREATE DATABASE CGNotes;
```

### マイグレーションエラー
アプリケーション起動時に自動的にマイグレーションが実行されます。
エラーが発生する場合は、ログを確認してください：

```bash
podman logs crystalgrowthnotebook2
```

### LAN内の他のPCからアクセスできない

1. **Windowsファイアウォールの確認**
   - ポート8080が許可されているか確認
   - `setup-firewall.bat` を管理者として実行

2. **コンテナが起動しているか確認**
   ```bash
   podman ps
   ```

3. **ポートバインディングの確認**
   ```bash
 podman ps | Select-String 8080
   ```
   `0.0.0.0:8080->8080/tcp` と表示されることを確認

4. **ネットワーク接続の確認**
```bash
   ping 192.168.11.23  # サーバーのIPアドレス
   ```

5. **ブラウザのキャッシュをクリア**
   - Ctrl+Shift+Delete でキャッシュクリア
   - シークレットモードで試す

## セキュリティ

- **`appsettings.json` は Gitにコミットしないこと**
- `.gitignore.custom` に除外設定済み
- パスワードは強力なものを使用
- 本番環境では環境変数やシークレット管理ツールの使用を推奨
- **LAN外部からのアクセスは推奨しません**（VPN等を使用してください）

## ネットワーク構成

### コンテナからホストのSQL Serverへアクセス

Windows/Macの場合、`host.docker.internal` を使用：

```json
{
  "ConnectionStrings": {
    "Default": "Server=host.docker.internal,1433;Database=CGNotes;..."
  }
}
```

### 外部SQL Serverへアクセス

IPアドレスまたはホスト名を指定：

```json
{
  "ConnectionStrings": {
    "Default": "Server=192.168.1.100,1433;Database=CGNotes;..."
  }
}
```

## チーム開発時の注意

1. **初回セットアップ**
   ```bash
 cp appsettings.json.template appsettings.json
   # appsettings.json を編集
   ```

2. **appsettings.json は各開発者のローカル環境に応じて設定**
   - Gitには含まれません
   - 各自で接続情報を設定してください

3. **設定の共有が必要な場合**
   - `appsettings.json.template` を更新
   - テンプレートのみGitにコミット
