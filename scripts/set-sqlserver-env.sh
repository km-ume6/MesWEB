#!/usr/bin/env bash
#
# Bash スクリプト: 環境変数の登録・更新・削除
#
# 使い方:
#   - 現在のシェルセッションで設定:
#       source ./set-sqlserver-env.sh 'Server=host,1433;Database=DB;User Id=user;Password=pass;Encrypt=False;'
#
#   - 環境変数を永続化（Linux の場合は ~/.profile などに追記）:
#       ./set-sqlserver-env.sh 'Server=...' --persist
#       その後、~/.profile または /etc/environment に追記する手順を表示します。
#
#   - 削除:
#       ./set-sqlserver-env.sh --remove
#
# 説明:
#   - このスクリプトは2つの環境変数を設定します:
#       SQLSERVER_CONNECTIONSTRING
#       ConnectionStrings__Default
#   - shell セッションで一時的に使う場合は source コマンドで読み込んでください。
#   - 永続化する場合は表示された手順に従ってください（ディストリビューションによってファイルが異なります）。

CONN="$1"
PERSIST=0
REMOVE=0

for arg in "$@"; do
  case "$arg" in
    --persist) PERSIST=1 ;;
    --remove) REMOVE=1 ;;
    *) ;;
  esac
done

if [ $REMOVE -eq 1 ]; then
  unset SQLSERVER_CONNECTIONSTRING
  unset ConnectionStrings__Default
  echo "Removed variables from current session."
  echo "To remove persisted entries, edit your shell profile files (e.g. ~/.profile, ~/.bashrc, /etc/environment)."
  exit 0
fi

if [ -z "$CONN" ]; then
  echo "Connection string is required. Usage: source ./set-sqlserver-env.sh 'Server=...;Database=...;User Id=...;Password=...;' [--persist]"
  exit 1
fi

# Set for current shell session
export SQLSERVER_CONNECTIONSTRING="$CONN"
export ConnectionStrings__Default="$CONN"

echo "Environment variables set for current session."

if [ $PERSIST -eq 1 ]; then
  echo
  echo "To persist these variables, add the following lines to your shell profile (e.g. ~/.profile or ~/.bashrc):"
  echo
  echo "export SQLSERVER_CONNECTIONSTRING='${CONN}'"
  echo "export ConnectionStrings__Default='${CONN}'"
  echo
  echo "Alternatively, for system-wide persistence, add them to /etc/environment (requires root)."
fi

# If script was sourced, variables remain in the shell. If executed, they will not persist.
