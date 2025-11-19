@echo off
setlocal enabledelayedexpansion
:: Export script for offline transfer of compose stack:: Usage: run on source PC from repo root (where docker-compose.yml lives)
echo Exporting containers and data for offline transfer...
:: Configurationset REPO_ROOT=%~dp0set PROJECT_DIR=CrystalGrowthNotebook2set EXPORT_NAME=crystalgrowth_offline_%DATE:~0,10%_%TIME:~0,2%-%TIME:~3,2%-%TIME:~6,2%set EXPORT_NAME=%EXPORT_NAME: =_%set EXPORT_DIR=%REPO_ROOT%offline_export_%RANDOM%
md "%EXPORT_DIR%"2>nul
if errorlevel1 (n echo Failed to create export dir %EXPORT_DIR%
 exit /b1
)
:: Build app image locally (tag for export)echo Building application image...
docker build -t crystalgrowthnotebook2_offline:latest -f "%PROJECT_DIR%\Dockerfile.simple" "%PROJECT_DIR%"
if %ERRORLEVEL% NEQ0 (
 echo ERROR: Failed to build application image
 exit /b1
)
:: Ensure official images are pullednecho Pulling required images (may be skipped if local)...docker pull mcr.microsoft.com/mssql/server:2022-latestndocker pull fauria/vsftpd:latest
:: Save images to tarnecho Saving images to %EXPORT_DIR%\images.tar ...docker save -o "%EXPORT_DIR%\images.tar" crystalgrowthnotebook2_offline:latest mcr.microsoft.com/mssql/server:2022-latest fauria/vsftpd:latest
if %ERRORLEVEL% NEQ0 (
 echo ERROR: docker save failed
 exit /b1
)
:: Export named volume sqlserver_datanecho Exporting docker volume 'sqlserver_data' ...docker run --rm -v sqlserver_data:/data -v "%EXPORT_DIR%":/backup alpine sh -c "cd /data && tar czf /backup/sqlserver_data.tar.gz ." 
if %ERRORLEVEL% NEQ0 (
 echo WARNING: Failed to export sqlserver_data volume. You may prefer to create a SQL .bak backup instead.
)
:: Copy host bind folders (FTPRoot and ForMount) if presentnecho Copying host bind folders if present...set HOST_FOR_MOUNT=D:\ForMountset HOST_FTPROOT=D:\ForMount\FTPRoot
if exist "%HOST_FOR_MOUNT%" ( echo Copying %HOST_FOR_MOUNT% ... powershell -Command "Compress-Archive -Path '%HOST_FOR_MOUNT%\*' -DestinationPath '%EXPORT_DIR%\\ForMount.zip' -Force" >nul) else ( echo Host folder %HOST_FOR_MOUNT% not found; skipping copy.)
if exist "%HOST_FTPROOT%" ( echo Copying %HOST_FTPROOT% ... powershell -Command "Compress-Archive -Path '%HOST_FTPROOT%\*' -DestinationPath '%EXPORT_DIR%\\FTPRoot.zip' -Force" >nul) else ( echo Host folder %HOST_FTPROOT% not found; skipping copy.)
:: Copy compose file and scriptsnecho Copying configuration files...copy "%REPO_ROOT%docker-compose.yml" "%EXPORT_DIR%" >nulif exist "%REPO_ROOT%CrystalGrowthNotebook2\run-compose.bat" copy "%REPO_ROOT%CrystalGrowthNotebook2\run-compose.bat" "%EXPORT_DIR%" >nulif exist "%REPO_ROOT%CrystalGrowthNotebook2\stop-compose.bat" copy "%REPO_ROOT%CrystalGrowthNotebook2\stop-compose.bat" "%EXPORT_DIR%" >nul
:: Create final ZIP archivenecho Creating final archive ...powershell -Command "Add-Type -AssemblyName 'System.IO.Compression.FileSystem'; [System.IO.Compression.ZipFile]::CreateFromDirectory('%EXPORT_DIR%', '%REPO_ROOT%\%EXPORT_NAME%.zip')"
if %ERRORLEVEL% EQU0 ( echo Export completed: %REPO_ROOT%%EXPORT_NAME%.zip) else ( echo ERROR: Failed to create final zip archive)
:: Cleanup temporary filesecho Cleaning up temporary files...del /q "%EXPORT_DIR%\images.tar" >nul2>&1del /q "%EXPORT_DIR%\sqlserver_data.tar.gz" >nul2>&1rd /s /q "%EXPORT_DIR%" >nul2>&1

echo Done.
pause
