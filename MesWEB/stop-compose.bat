@echo off
echo ========================================
echo Crystal Growth Notebook 2 - Stop Containers
echo ========================================

REM Require administrator privileges to modify firewall rules
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
 echo ADMINISTRATOR PRIVILEGES REQUIRED
 echo Please run this script as Administrator.
 pause
 exit /b 1
)

echo Stopping containers...

REM Prefer modern docker compose if available
if defined DOCKER_CMD (
 "%DOCKER_CMD%" compose down
) else (
 docker compose down 2>nul || docker-compose down
)

if errorlevel 1 (
 echo ERROR: Failed to stop containers!
 pause
 exit /b 1
)

echo.
echo Removing Windows Firewall rules added by run-compose.bat...
rem Rule names created by run-compose.bat
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port8080" >nul 2>&1
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port1433" >nul 2>&1
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port21" >nul 2>&1
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port20" >nul 2>&1
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port2222" >nul 2>&1
netsh advfirewall firewall delete rule name="Crystalgrowth Docker Port21100-21110" >nul 2>&1

echo.
echo Firewall rules removed and containers stopped successfully!
echo.
echo To remove volumes (delete SQL Server data):
echo docker compose down -v
echo.

pause
