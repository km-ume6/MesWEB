# Crystal Growth Notebook 2 - Docker Compose セットアップ

## 概要

このガイドでは、**SQL Server と Blazor アプリケーションの両方をコンテナで実行**する方法を説明します。

SQL Serverのデータは **`D:\MSSQL\Data`** に保存されます。

## 前提条件

- **Docker Desktop** または **Rancher Desktop** がインストールされていること
- **.NET 9 SDK** がインストールされていること（ビルド用）
- **Dドライブ**が利用可能であること

## アーキテクチャ

```
┌────────────────────────────────────────────┐
│   Docker Compose               │
│                                │
│  ┌──────────────────────────────────────┐  │
│  │  crystalgrowthnotebook2      │  │
│  │  (Blazor App)    │  │
│  │  Port: 8080                  │  │
│  └──────────────────────────────────────┘  │
│    ↓   ↓
│  ┌──────────────────────────────────────┐  │
│  │  sqlserver    │
│  │  (SQL Server 2022)           │  │
│  │  Port: 1433      │  │
│  │  Data: D:\MSSQL\Data ←ホスト│  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
              ↓
        D:\MSSQL\Data
     (ホストのディスク)
```

## クイックスタート

### 1. データディレクトリの準備（初回のみ）

**自動作成（推奨）:**

`run-compose.bat` を実行すると自動的に `D:\MSSQL\Data` を作成します。

**手動作成:**

```cmd
mkdir D:\MSSQL\Data
```

### 2. コンテナを起動

```cmd
cd CrystalGrowthNotebook2
.\run-compose.bat
```

または Linux/Mac:

```bash
cd CrystalGrowthNotebook2
chmod +x run-compose.sh
./run-compose.sh
```

### 3. アクセス

- **アプリケーション**: http://localhost:8080
- **SQL Server**: localhost:1433

### 4. 停止

```cmd
.\stop-compose.bat
```

または:

```bash
docker-compose down
```

## 詳細設定

### SQL Server

**デフォルト設定:**
- **Image**: mcr.microsoft.com/mssql/server:2022-latest
- **SA Password**: `YourStrong!Passw0rd123`
- **Database**: `CGNotes`（初回作成）
- **Port**: 1433
- **Data Location**: `D:\MSSQL\Data`（ホストのディスク）

**データの保存場所:**

| OS | パス |
|----|------|
| **Windows** | `D:\MSSQL\Data` |
| **WSL2** | `/mnt/d/MSSQL/Data` |
| **Linux/Mac** | Docker Volumeを使用（要変更） |

**パスワードを変更する場合:**

1. `docker-compose.yml` を編集：
   ```yaml
   environment:
     - MSSQL_SA_PASSWORD=YourNewPassword
   ```

2. `appsettings.Container.json` を編集：
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=sqlserver,1433;Database=CGNotes;User Id=sa;Password=YourNewPassword;..."
   }
   }
   ```

**データの保存場所を変更する場合:**

`docker-compose.yml` を編集：

```yaml
volumes:
  # 別のパスに変更
  - E:/SQLData:/var/opt/mssql
```

### アプリケーション

**設定ファイル:**
- コンテナ実行時は `appsettings.Container.json` を使用します
- サーバー名は `sqlserver`（Docker Composeのサービス名）

## データの管理

### データの永続化

SQL Serverのデータは **ホストの `D:\MSSQL\Data`** に保存されるため：

- ✅ **コンテナを削除してもデータは残る**
- ✅ **直接ファイルにアクセス可能**
- ✅ **バックアップが簡単**
- ✅ **ディスク容量の管理が容易**

### データの確認

```cmd
dir D:\MSSQL\Data
```

以下のようなファイルが作成されます：

```
D:\MSSQL\Data
├── master.mdf
├── mastlog.ldf
├── model.mdf
├── modellog.ldf
├── msdbdata.mdf
├── msdblog.ldf
├── tempdb.mdf
├── templog.ldf
├── CGNotes.mdf    ← アプリケーションのデータベース
└── CGNotes_log.ldf
```

### データのバックアップ

**方法1: ファイルコピー（簡単）**

```cmd
# コンテナ停止
docker-compose stop sqlserver

# フォルダごとコピー
xcopy D:\MSSQL\Data D:\Backup\MSSQL\Data_%date:~0,4%%date:~5,2%%date:~8,2% /E /I

# コンテナ再起動
docker-compose start sqlserver
```

**方法2: SQL Serverバックアップ（推奨）**

```bash
# コンテナ内でバックアップ
docker exec crystalgrowth-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd123 -Q "BACKUP DATABASE CGNotes TO DISK='/var/opt/mssql/CGNotes_backup.bak' WITH FORMAT" -C

# バックアップファイルがD:\MSSQL\Dataに作成される
dir D:\MSSQL\Data\*.bak
```

### データのリストア

```bash
# バックアップファイルがD:\MSSQL\Dataにある場合
docker exec crystalgrowth-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd123 -Q "RESTORE DATABASE CGNotes FROM DISK='/var/opt/mssql/CGNotes_backup.bak' WITH REPLACE" -C
```

### データのクリーンアップ

**完全削除（データベースを削除）:**

```cmd
# コンテナ停止・削除
docker-compose down

# データフォルダ削除
rmdir /S /Q D:\MSSQL\Data

# 再度起動すると新規データベースが作成される
.\run-compose.bat
```

## コマンド一覧

### 起動・停止

```bash
# すべてのコンテナを起動
docker-compose up -d

# すべてのコンテナを停止
docker-compose stop

# すべてのコンテナを停止して削除（データは残る）
docker-compose down
```

### ログ確認

```bash
# すべてのログを表示
docker-compose logs -f

# SQL Serverのログのみ
docker-compose logs -f sqlserver

# アプリケーションのログのみ
docker-compose logs -f crystalgrowthnotebook2
```

### コンテナの状態確認

```bash
# 実行中のコンテナを表示
docker-compose ps

# すべてのコンテナを表示
docker-compose ps -a
```

### SQL Serverに接続

#### Azure Data Studio または SQL Server Management Studio

- **Server**: localhost,1433
- **Authentication**: SQL Server Authentication
- **User**: sa
- **Password**: YourStrong!Passw0rd123

#### コマンドライン（sqlcmd）

```bash
# コンテナ内で実行
docker exec -it crystalgrowth-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd123 -C

# データベース一覧
SELECT name FROM sys.databases;
GO

# テーブル一覧
USE CGNotes;
GO
SELECT name FROM sys.tables;
GO
```

## トラブルシューティング

### D:\MSSQL\Dataが作成できない

**権限エラー:**

```cmd
# 管理者としてコマンドプロンプトを開いて実行
mkdir D:\MSSQL\Data
```

**Dドライブが存在しない:**

別のドライブを使用する場合、`docker-compose.yml` を編集：

```yaml
volumes:
  - C:/MSSQL/Data:/var/opt/mssql
```

### SQL Serverコンテナが起動しない

**メモリ不足:**
- SQL Serverは最低2GBのメモリが必要です
- Docker Desktopの設定でメモリを増やしてください

**ポート競合:**
```bash
# ポート1433が使用中か確認
netstat -ano | findstr :1433

# 使用中の場合、docker-compose.ymlでポートを変更
ports:
  - "1434:1433"
```

**データファイルの権限エラー:**

```cmd
# D:\MSSQL\Dataの権限を確認
icacls D:\MSSQL\Data

# 必要に応じて権限を付与
icacls D:\MSSQL\Data /grant Everyone:(OI)(CI)F
```

### アプリケーションがSQL Serverに接続できない

**ヘルスチェック確認:**
```bash
docker-compose ps
# sqlserver が "healthy" になるまで待つ
```

**ネットワーク確認:**
```bash
# ネットワーク一覧
docker network ls

# コンテナのネットワーク接続を確認
docker network inspect crystalgrowthnotebook2_crystalgrowth-network
```

**接続テスト:**
```bash
# アプリケーションコンテナからSQL Serverにping
docker exec crystalgrowthnotebook2 ping sqlserver
```

### データベースが作成されない

マイグレーションは自動的に実行されますが、手動で確認・実行する場合：

```bash
# アプリケーションのログを確認
docker-compose logs crystalgrowthnotebook2

# SQL Serverに直接接続して確認
docker exec -it crystalgrowth-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd123 -C

# データベースを手動で作成
CREATE DATABASE CGNotes;
GO
```

## LAN内の他のPCからアクセス

### ファイアウォール設定

```cmd
.\setup-firewall.bat
```
（管理者として実行）

### アクセスURL

```
http://192.168.11.23:8080
```
（IPアドレスは実際の値に置き換え）

## 本番環境での推奨設定

### セキュリティ

1. **SAパスワードを変更**
   - 複雑なパスワードに変更

2. **専用ユーザーを作成**
   ```sql
   CREATE LOGIN cgn_user WITH PASSWORD = 'StrongPassword123!';
   USE CGNotes;
   CREATE USER cgn_user FOR LOGIN cgn_user;
   ALTER ROLE db_owner ADD MEMBER cgn_user;
   ```

3. **TLS/SSL を有効化**
   ```yaml
   environment:
     - Encrypt=True
   ```

4. **データフォルダの権限を制限**
   ```cmd
   icacls D:\MSSQL\Data /inheritance:r
   icacls D:\MSSQL\Data /grant Administrators:F
   ```

### パフォーマンス

1. **メモリ制限を設定**
   ```yaml
   sqlserver:
     deploy:
       resources:
         limits:
           memory: 4G
         reservations:
           memory: 2G
   ```

2. **バックアップスケジュール**
   - Windows Task Scheduler で定期バックアップ

3. **SSDの使用**
   - `D:\MSSQL\Data` をSSDに配置

## ディスク容量管理

### 現在の使用量確認

```cmd
dir D:\MSSQL\Data
```

### ログファイルのサイズ管理

```sql
-- ログファイルのサイズを確認
USE CGNotes;
GO
EXEC sp_helpfile;
GO

-- ログファイルを縮小
DBCC SHRINKFILE (CGNotes_log, 100);
GO
```

### 古いバックアップの削除

```cmd
# 30日以前のバックアップを削除
forfiles /p D:\MSSQL\Data /m *.bak /d -30 /c "cmd /c del @path"
```

## 次のステップ

- **監視**: Prometheus + Grafana で監視
- **バックアップ**: 定期バックアップの自動化（Task Scheduler）
- **ストレージ**: RAID構成でデータ保護
- **パフォーマンス**: SSDへの移行
- **CI/CD**: GitHub Actions でのデプロイ自動化

## 参考リンク

- [Docker Compose ドキュメント](https://docs.docker.com/compose/)
- [SQL Server on Linux](https://learn.microsoft.com/ja-jp/sql/linux/sql-server-linux-overview)
- [ASP.NET Core Docker イメージ](https://hub.docker.com/_/microsoft-dotnet-aspnet)
