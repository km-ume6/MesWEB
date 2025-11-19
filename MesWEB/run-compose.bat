@echo off
setlocal enabledelayedexpansion

REM Check for admin privileges early (required for Docker daemon access on Windows)
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
 echo.
 echo ========================================
 echo ADMINISTRATOR PRIVILEGES REQUIRED
 echo ========================================
 echo This script needs administrator privileges to access Docker daemon on Windows.
 echo Please right-click this script and select "Run as Administrator".
 echo.
 pause
 exit /b 1
)

REM カスタマイズ可能な設定
if not defined WSL_DISTRO set "WSL_DISTRO=rancher-desktop"
if not defined RANCHER_PATH_1 set "RANCHER_PATH_1=%LOCALAPPDATA%\Programs\Rancher Desktop\Rancher Desktop.exe"
if not defined RANCHER_PATH_2 set "RANCHER_PATH_2=C:\Program Files\Rancher Desktop\Rancher Desktop.exe"
if not defined DOCKER_WAIT_TIME set "DOCKER_WAIT_TIME=120"

REM スクリプトの場所を基準にプロジェクトパスを探索（任意の作業ディレクトリから実行可能）
set "SCRIPT_DIR=%~dp0"
rem Remove trailing backslash for easier path manipulation
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

REM Find the project directory by searching upwards for CrystalGrowthNotebook2.csproj
set "CUR_DIR=%SCRIPT_DIR%"
set "PROJECT_DIR="
:find_proj
if exist "%CUR_DIR%\CrystalGrowthNotebook2.csproj" (
 set "PROJECT_DIR=%CUR_DIR%"
) else (
 rem Stop if we reached drive root
 if "%CUR_DIR%"=="%CUR_DIR:~0,3%" goto proj_not_found
 for %%I in ("%CUR_DIR%\..") do set "CUR_DIR=%%~fI"
 goto find_proj
)

goto proj_found

:proj_not_found
echo ERROR: Could not locate CrystalGrowthNotebook2.csproj starting from %SCRIPT_DIR% upwards.
pause
exit /b 1

:proj_found
set "PARENT_DIR=%PROJECT_DIR%\.."

echo ========================================
echo Crystal Growth Notebook2 - Docker Compose
echo ========================================
echo Script directory: %SCRIPT_DIR%
echo Project directory: %PROJECT_DIR%
echo Compose working dir (parent): %PARENT_DIR%
echo WSL Distribution: %WSL_DISTRO%
echo Rancher Path1: %RANCHER_PATH_1%
echo Rancher Path2: %RANCHER_PATH_2%
echo Docker Wait Time: %DOCKER_WAIT_TIME% seconds
echo ========================================

REM Dockerコマンドのパスを設定
set "DOCKER_CMD=docker"
if exist "C:\Program Files\Rancher Desktop\resources\resources\win32\bin\docker.exe" (
 set "DOCKER_CMD=C:\Program Files\Rancher Desktop\resources\resources\win32\bin\docker.exe"
)

REM Dockerデーモンの起動確認
echo [0/5] Checking Docker daemon...

echo Checking %WSL_DISTRO% WSL docker availability...

REM Check WSL distro state
set "WSL_STATE="
for /f "tokens=3" %%S in ('wsl -l -v 2^>nul ^| findstr /i "%WSL_DISTRO%"') do set "WSL_STATE=%%S"
if not defined WSL_STATE (
 echo WSL distro %WSL_DISTRO% not found or wsl not available.
) else (
 echo WSL distro %WSL_DISTRO% state: %WSL_STATE%
)

REM If Stopped, try to start via wsl then wait for Running
if defined WSL_STATE if /i "%WSL_STATE%"=="Stopped" (
 echo WSL distro %WSL_DISTRO% is stopped. Attempting to start via wsl...
 wsl -d "%WSL_DISTRO%" -- bash -lc "exit" >nul 2>&1 || echo could not start via wsl command
 set /a wcount=0
 goto wait_wsl
)

REM After attempting start (or if state was not Stopped), check docker in WSL
wsl -d "%WSL_DISTRO%" -- docker ps >nul 2>&1
if %ERRORLEVEL%==0 (
 echo %WSL_DISTRO% WSL is running and docker is responding. Skipping Rancher start.
 goto dockerready
)

REM If WSL/docker not available, try to launch Rancher Desktop GUI
echo %WSL_DISTRO% WSL/docker not available yet.
if exist "%RANCHER_PATH_1%" (
 start "" "%RANCHER_PATH_1%"
 echo Starting Rancher Desktop via GUI...
) else if exist "%RANCHER_PATH_2%" (
 start "" "%RANCHER_PATH_2%"
 echo Starting Rancher Desktop via GUI...
) else (
 echo ERROR: Rancher Desktop not found at specified paths!
 echo Path1: %RANCHER_PATH_1%
 echo Path2: %RANCHER_PATH_2%
 echo Please check your Rancher Desktop installation or set custom paths.
 pause
 exit /b 1
)

:wait_for_docker
REM 最大%DOCKER_WAIT_TIME%秒待機（2秒間隔）
set /a counter=0
:waitloop
timeout /t 2 /nobreak >nul

REM Check WSL docker first (Windows host docker may need admin, so check WSL first)
wsl -d "%WSL_DISTRO%" -- docker ps >nul 2>&1
if %ERRORLEVEL%==0 (
 echo %WSL_DISTRO% WSL docker responding.
 goto dockerready
)

REM Check host docker (needs admin)
"%DOCKER_CMD%" ps >nul 2>&1
if %ERRORLEVEL%==0 (
 echo Host docker responding.
 goto dockerready
)

set /a counter+=2
if %counter% lss %DOCKER_WAIT_TIME% (
 echo Still waiting... (%counter%s)
 goto waitloop
)

echo ERROR: Rancher Desktop did not start within %DOCKER_WAIT_TIME% seconds.
echo Please start Rancher Desktop manually and try again.
pause
exit /b 1

:wait_wsl
REM wait loop for WSL to become Running (10 seconds max)
set /a wcount=0
:wait_wsl_loop
timeout /t 1 >nul
for /f "tokens=3" %%S in ('wsl -l -v 2^>nul ^| findstr /i "%WSL_DISTRO%"') do set "WSL_STATE=%%S"
if /i "%WSL_STATE%"=="Running" goto wsl_started
set /a wcount+=1
if %wcount% GEQ 10 goto wsl_not_started
goto wait_wsl_loop

:wsl_started
echo WSL distro %WSL_DISTRO% is now running.
REM proceed to docker check
wsl -d "%WSL_DISTRO%" -- docker ps >nul 2>&1
if %ERRORLEVEL%==0 (
 echo %WSL_DISTRO% WSL is running and docker is responding. Skipping Rancher start.
 goto dockerready
)

echo WSL running but docker not yet available; will attempt GUI start and continue waiting.
if exist "%RANCHER_PATH_1%" (
 start "" "%RANCHER_PATH_1%"
) else if exist "%RANCHER_PATH_2%" (
 start "" "%RANCHER_PATH_2%"
)

goto wait_for_docker

:wsl_not_started
echo WSL did not start in time; attempting to start Rancher Desktop GUI.
if exist "%RANCHER_PATH_1%" (
 start "" "%RANCHER_PATH_1%"
) else if exist "%RANCHER_PATH_2%" (
 start "" "%RANCHER_PATH_2%"
)

goto wait_for_docker

:dockerready
echo Docker daemon is running!

echo.
echo [1/4] Preparing configuration...

REM プロジェクトのパブリッシュ（プロジェクトフォルダから実行）
if not exist "%PROJECT_DIR%" (
 echo ERROR: Project directory not found: %PROJECT_DIR%
 pause
 exit /b 1
)

pushd "%PROJECT_DIR%"
echo Publishing project in %CD% ...

dotnet publish "CrystalGrowthNotebook2.csproj" -c Release -o bin\Release\net9.0\publish
if %ERRORLEVEL% NEQ 0 (
 echo ERROR: Build/publish failed!
 popd
 pause
 exit /b 1
)

popd

REM Docker Compose 実行ディレクトリ（親ディレクトリ）に移動して docker-compose.yml を探す
pushd "%PARENT_DIR%"
set "COMPOSE_FILE=%CD%\docker-compose.yml"
if not exist "%COMPOSE_FILE%" (
 echo ERROR: docker-compose.yml not found in %CD%
 echo Expected at: %COMPOSE_FILE%
 popd
 pause
 exit /b 1
)

echo Using compose file: %COMPOSE_FILE%

echo [3/4] Stopping existing containers (if any)...

REM Use Windows host docker compose (admin required) instead of WSL
echo Running docker compose...
"%DOCKER_CMD%" compose -f "%COMPOSE_FILE%" down

REM Open required firewall ports so services are reachable while containers are up
set "FW_PREFIX=Crystalgrowth Docker Port"
echo [3.5/4] Opening Windows Firewall ports for containers...
rem Common ports used by compose:8080 (app),1433 (sql),21/20 and21100-21110 (ftp)
netsh advfirewall firewall add rule name="%FW_PREFIX%8080" dir=in action=allow protocol=TCP localport=8080 profile=any >nul 2>&1
netsh advfirewall firewall add rule name="%FW_PREFIX%1433" dir=in action=allow protocol=TCP localport=1433 profile=any >nul 2>&1
netsh advfirewall firewall add rule name="%FW_PREFIX%21" dir=in action=allow protocol=TCP localport=21 profile=any >nul 2>&1
netsh advfirewall firewall add rule name="%FW_PREFIX%20" dir=in action=allow protocol=TCP localport=20 profile=any >nul 2>&1
netsh advfirewall firewall add rule name="%FW_PREFIX%21100-21110" dir=in action=allow protocol=TCP localport=21100-21110 profile=any >nul 2>&1

echo [4/4] Starting containers...
"%DOCKER_CMD%" compose -f "%COMPOSE_FILE%" up -d
if %ERRORLEVEL% NEQ 0 (
 echo ERROR: Failed to start containers!
 "%DOCKER_CMD%" compose -f "%COMPOSE_FILE%" logs --tail=50
 popd
 pause
 exit /b 1
)

echo.
echo ========================================
echo SUCCESS! Containers started!
echo ========================================
echo.
echo SQL Server:
echo Host: localhost
echo Port:1433
echo User: sa
echo Password: YourStrong!Passw0rd123
echo Database: CGNotes (auto-create)
echo Data Location: Container default (/var/opt/mssql)
echo.
echo Application:
echo Local: http://localhost:8080
echo.
echo LAN内の他のPCからアクセス:
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4" 2^>nul') do (
 set "ip=%%a"
 set "ip=!ip: =!"
 echo http://!ip!:8080
)

echo.
echo Useful commands:
echo %DOCKER_CMD% compose logs -f ^# View all logs
echo %DOCKER_CMD% compose logs -f sqlserver ^# View SQL Server logs
echo %DOCKER_CMD% compose logs -f crystalgrowthnotebook2 ^# View app logs
echo %DOCKER_CMD% compose stop ^# Stop containers
echo %DOCKER_CMD% compose start ^# Start containers
echo %DOCKER_CMD% compose down ^# Stop and remove containers
echo.
echo SQL Server data is stored in container default path.
echo.

popd
pause
