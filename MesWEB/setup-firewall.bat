@echo off
echo ========================================
echo Crystal Growth Notebook - Firewall Setup
echo ========================================
echo.
echo このスクリプトは管理者権限が必要です。
echo ポート8080をWindows Firewallで開放します。
echo.
pause

REM 既存の規則を削除（あれば）
netsh advfirewall firewall delete rule name="Crystal Growth Notebook (Podman)" >nul 2>&1

REM ファイアウォール規則を追加（すべてのプロファイルで有効）
netsh advfirewall firewall add rule name="Crystal Growth Notebook (Podman)" dir=in action=allow protocol=TCP localport=8080 profile=private,domain,public

if errorlevel 1 (
    echo.
    echo ERROR: ファイアウォール規則の追加に失敗しました。
    echo このバッチファイルを「管理者として実行」してください。
    echo.
) else (
    echo.
    echo SUCCESS! ファイアウォール規則を追加しました。
    echo.
    echo 設定内容:
    echo   - プロトコル: TCP
    echo - ポート: 8080
    echo   - プロファイル: Private, Domain, Public
    echo.
    echo LAN内の他のPCから以下のURLでアクセスできます:
    
    REM このPCのIPアドレスを表示
    echo.
    echo このPCのIPアドレス:
    for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4"') do echo   http://%%a:8080
    echo.
)

pause
