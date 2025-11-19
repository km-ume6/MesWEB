# Rancher Desktop インストール手順

## 1. Rancher Desktop のダウンロード

https://rancherdesktop.io/
から最新版をダウンロード

または、winget でインストール：

```powershell
winget install suse.RancherDesktop
```

## 2. 初回起動時の設定

1. **Container Runtime を選択**
   - 「dockerd (moby)」を選択（Docker CLI互換のため）

2. **Kubernetes を無効化**（オプション）
   - 不要な場合は「Kubernetes を有効にする」のチェックを外す

3. **WSL2 統合**
   - デフォルトのままでOK

## 3. 確認

```powershell
docker --version
docker ps
```

## 4. 既存の run-podman.bat を run-docker.bat に変更

podman コマンドを docker に置き換えるだけです。

## 5. アプリケーション起動

```cmd
.\run-docker.bat
```

## トラブルシューティング

### WSL2 が必要
Rancher Desktop は WSL2 を使用するため、事前にインストールが必要です：

```powershell
wsl --install
```

### PATH の確認
Rancher Desktop のインストール後、Docker CLI が使えない場合：

```powershell
# Rancher Desktop の設定 → WSL → PATH統合 を確認
```
