@echo off
setlocal enabledelayedexpansion
:: Import script for offline transfer (run on target PC):: Usage: place the zip produced by export-offline.bat next to this script and run as Administrator.
if "%~1"=="" ( echo Usage: %~nx0 exported_archive.zip exit /b1)
set ZIPFILE=%~1
set WORKDIR=%~dp0import_temp
md "%WORKDIR%"2>nul
:: Extract archivenecho Extracting %ZIPFILE% to %WORKDIR%...powershell -Command "Add-Type -AssemblyName 'System.IO.Compression.FileSystem'; [System.IO.Compression.ZipFile]::ExtractToDirectory('%ZIPFILE%', '%WORKDIR%')"
if %ERRORLEVEL% NEQ0 ( echo ERROR: Failed to extract archive exit /b1)
:: Load imagesif exist "%WORKDIR%\images.tar" ( echo Loading docker images from images.tar...docker load -i "%WORKDIR%\images.tar") else ( echo images.tar not found; skipping image load.)
:: Restore volume if presentif exist "%WORKDIR%\sqlserver_data.tar.gz" ( echo Restoring docker volume 'sqlserver_data'...docker run --rm -v sqlserver_data:/data -v "%WORKDIR%":/backup alpine sh -c "cd /data && tar xzf /backup/sqlserver_data.tar.gz" ) else ( echo sqlserver_data backup not found; skipping volume restore.)
:: Extract host bind archives if presentif exist "%WORKDIR%\ForMount.zip" ( echo Restoring ForMount to C:\ForMount ... powershell -Command "Expand-Archive -Path '%WORKDIR%\\ForMount.zip' -DestinationPath 'C:\ForMount' -Force" >nul) else ( echo ForMount.zip not found; create host folders manually if needed.)if exist "%WORKDIR%\FTPRoot.zip" ( echo Restoring FTPRoot to C:\ForMount\FTPRoot ... powershell -Command "Expand-Archive -Path '%WORKDIR%\\FTPRoot.zip' -DestinationPath 'C:\ForMount\\FTPRoot' -Force" >nul)
:: Copy compose file and scripts to current dirif exist "%WORKDIR%\docker-compose.yml" copy "%WORKDIR%\docker-compose.yml" "%CD%" >nulif exist "%WORKDIR%\run-compose.bat" copy "%WORKDIR%\run-compose.bat" "%CD%" >nulif exist "%WORKDIR%\stop-compose.bat" copy "%WORKDIR%\stop-compose.bat" "%CD%" >nul
:: Notify user to adjust settingsnecho Import complete. Please review docker-compose.yml and ensure PASV_ADDRESS and bind mount paths match this host.
echo Then run run-compose.bat as Administrator to start the stack.pause
