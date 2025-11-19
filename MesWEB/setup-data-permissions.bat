@echo off
echo ========================================
echo SQL Server Data Directory - Permission Setup
echo ========================================
echo.
echo このスクリプトは管理者権限で実行する必要があります。
echo D:\MSSQL\Data に Docker/WSL2 からアクセスできるように権限を設定します。
echo.
pause

REM D:\MSSQL\Dataディレクトリが存在するか確認
if not exist "D:\MSSQL\Data" (
    echo ディレクトリが存在しません。作成します...
  mkdir "D:\MSSQL\Data"
    if errorlevel 1 (
   echo ERROR: ディレクトリの作成に失敗しました
  pause
        exit /b 1
    )
  echo ディレクトリを作成しました: D:\MSSQL\Data
)

echo ディレクトリが存在します: D:\MSSQL\Data

echo.
echo [1/2] Everyone にフルコントロール権限を付与（再帰的）...
icacls "D:\MSSQL\Data" /grant Everyone:(OI)(CI)F /T

if errorlevel 1 (
    echo.
    echo ERROR: 権限の設定に失敗しました
    echo このスクリプトを「管理者として実行」してください
    echo.
    pause
    exit /b 1
)

echo.
echo [2/2] 現在の権限を確認...
icacls "D:\MSSQL\Data"

echo.
echo ========================================
echo SUCCESS! 権限設定完了
echo ========================================
echo.
echo D:\MSSQL\Data は Docker/WSL2 からアクセス可能になりました。
echo.
echo 次のステップ:
echo   .\run-compose.bat を実行してコンテナを起動してください
echo.

pause
