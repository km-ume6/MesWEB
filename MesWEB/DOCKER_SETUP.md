# Docker セットアップガイド（Rancher Desktop）

## 前提条件

- Windows 10/11 Pro, Enterprise, または Education（WSL2対応）
- WSL2 がインストール済み

## WSL2 のインストール（まだの場合）

PowerShell を管理者として開いて実行：

```powershell
wsl --install
```

再起動後、Linux ディストリビューション（Ubuntuなど）をセットアップ。

## Rancher Desktop のインストール

### 方法1: winget（推奨）

```powershell
winget install SUSE.RancherDesktop
```

### 方法2: 手動ダウンロード

https://rancherdesktop.io/ から最新版をダウンロード

## 初回起動時の設定

1. **Rancher Desktop を起動**

2. **Welcome 画面で設定**:
   - ✅ **Container Runtime**: `dockerd (moby)` を選択
   - ✅ **Enable Kubernetes**: チェックを外す（不要な場合）
   - ✅ **WSL**: デフォルトのまま

3. **Start** をクリック

4. **起動完了を待つ**（初回は5-10分程度）
   - タスクトレイのアイコンが緑色になれば完了

## 確認

新しい PowerShell を開いて：

```powershell
docker --version
docker ps
```

または、確認スクリプトを実行：

```cmd
.\CrystalGrowthNotebook2\check-docker.bat
```

## アプリケーションの起動

Docker が使えるようになったら：

```cmd
cd CrystalGrowthNotebook2
.\run-docker.bat
```

## トラブルシューティング

### docker コマンドが見つからない

1. **Rancher Desktop が起動しているか確認**
   - タスクトレイのアイコンを確認

2. **PowerShell/ターミナルを再起動**
   - PATH が更新されていない可能性

3. **WSL Integration を確認**
   - Rancher Desktop の Preferences → WSL → Integrations
   - 使用する WSL ディストリビューションにチェック

### Docker デーモンが起動しない

1. **WSL2 のバージョンを確認**
   ```powershell
   wsl --list --verbose
   ```
   バージョンが 2 になっているか確認

2. **Rancher Desktop を再起動**

3. **ログを確認**
   - Rancher Desktop → Troubleshooting → View Logs

### ポート8080でアクセスできない

Dockerの場合は問題ないはずですが、確認：

```powershell
docker port crystalgrowthnotebook2
netstat -ano | findstr :8080
```

## Podman との違い

| 項目 | Podman | Docker (Rancher Desktop) |
|------|--------|--------------------------|
| LAN アクセス | ❌ 困難（127.0.0.1のみ） | ✅ 可能（0.0.0.0） |
| セットアップ | 簡単 | やや複雑（WSL2必要） |
| 互換性 | Docker互換（ほぼ） | Docker標準 |
| GUI | なし | あり |

## 次のステップ

1. **ファイアウォール設定**（LAN アクセス用）
   ```cmd
   .\setup-firewall.bat
   ```
   （管理者として実行）

2. **アプリケーション起動**
   ```cmd
   .\run-docker.bat
   ```

3. **ブラウザでアクセス**
   ```
   http://localhost:8080
   ```

4. **LAN内の他のPCからアクセス**
   ```
   http://192.168.11.23:8080
   ```
   （IPアドレスは実際の値に置き換え）

## 参考リンク

- [Rancher Desktop 公式サイト](https://rancherdesktop.io/)
- [Docker ドキュメント](https://docs.docker.com/)
- [WSL2 ドキュメント](https://learn.microsoft.com/ja-jp/windows/wsl/)
