@echo off
chcp 65001 >nul
color 0A
title NGO Demo System - Smart Mode
cls

echo.
echo ================================================================
echo.
echo             NGO Management System - Smart Mode
echo.
echo         Fixed Domain: ngo-management-hub.com
echo.
echo ================================================================
echo.
echo  Features:
echo    * Only 4 windows (3 services + control)
echo    * Fixed public URLs (no more random URLs!)
echo    * MVC Portal:  https://ngo-management-hub.com
echo    * Admin Panel: https://admin.ngo-management-hub.com
echo    * Backend API: https://api.ngo-management-hub.com
echo.
echo ================================================================
echo.
pause

REM Create logs directory
set "LOGS_DIR=%~dp0..\logs"
if not exist "%LOGS_DIR%" mkdir "%LOGS_DIR%"

REM ================================================================
REM Step 1: Cleanup old processes
REM ================================================================
cls
echo.
echo ================================================================
echo  [1/5] Cleaning up old processes...
echo ================================================================
echo.

taskkill /F /IM node.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM cloudflared.exe >nul 2>&1

REM Clean old logs
del "%LOGS_DIR%\tunnel-*.log" >nul 2>&1

timeout /t 2 /nobreak >nul
echo  [OK] Old processes cleaned
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM Step 2: Start Backend WebAPI
REM ================================================================
cls
echo.
echo ================================================================
echo  [2/5] Starting Backend WebAPI...
echo ================================================================
echo.

cd /d "%~dp0..\backend"
start "Backend API (localhost:5264)" cmd /k "color 0B && echo ========================================= && echo   Backend WebAPI - http://localhost:5264 && echo ========================================= && echo. && dotnet run"

echo  Starting backend service...
timeout /t 12 /nobreak >nul
echo  [OK] Backend WebAPI started
echo.

REM ================================================================
REM Step 3: Start React Admin Panel
REM ================================================================
cls
echo.
echo ================================================================
echo  [3/5] Starting React Admin Panel...
echo ================================================================
echo.

cd /d "%~dp0..\frontend"
start "React Admin (localhost:5173)" cmd /k "color 0D && echo ========================================= && echo   React Admin - http://localhost:5173 && echo ========================================= && echo. && npm run dev"

echo  Starting React Admin Panel...
timeout /t 8 /nobreak >nul
echo  [OK] React Admin Panel started
echo.

REM ================================================================
REM Step 4: Start MVC User Portal
REM ================================================================
cls
echo.
echo ================================================================
echo  [4/5] Starting MVC User Portal...
echo ================================================================
echo.

cd /d "%~dp0..\mvc-frontend\NGOPlatformWeb"
start "MVC Portal (localhost:5066)" cmd /k "color 0E && echo ========================================= && echo   MVC Portal - http://localhost:5066 && echo ========================================= && echo. && dotnet run"

echo  Starting MVC User Portal...
timeout /t 12 /nobreak >nul
echo  [OK] MVC User Portal started
echo.

REM ================================================================
REM Step 5: Start Cloudflare Named Tunnel (Fixed URLs)
REM ================================================================
cls
echo.
echo ================================================================
echo  [5/5] Starting Cloudflare Named Tunnel...
echo ================================================================
echo.
echo  Starting tunnel with fixed domain: ngo-management-hub.com
echo.

cd /d "%~dp0"

REM Start Named Tunnel (single tunnel handles all 3 services)
start /B "" cmd /c "cloudflared tunnel run ngo-demo > "%LOGS_DIR%\tunnel.log" 2>&1"

echo  Waiting for tunnel to establish (10 seconds)...
echo.
echo  [                    ] 0%%
timeout /t 2 /nobreak >nul
echo  [====                ] 20%%
timeout /t 2 /nobreak >nul
echo  [========            ] 40%%
timeout /t 2 /nobreak >nul
echo  [============        ] 60%%
timeout /t 2 /nobreak >nul
echo  [================    ] 80%%
timeout /t 2 /nobreak >nul
echo  [====================] 100%%
echo.
echo  [OK] Tunnel started
timeout /t 2 /nobreak >nul

REM ================================================================
REM Show Results
REM ================================================================
:show_results
cls
echo.
echo ================================================================
echo.
echo            System Started Successfully!
echo.
echo ================================================================
echo.
echo ================================================================
echo  PUBLIC ADDRESSES (Fixed - ngo-management-hub.com)
echo ================================================================
echo.
echo   MVC User Portal:   https://ngo-management-hub.com
echo   React Admin:       https://admin.ngo-management-hub.com
echo   Backend API:       https://api.ngo-management-hub.com
echo.
echo ================================================================
echo  LOCAL ADDRESSES
echo ================================================================
echo.
echo   Backend API:       http://localhost:5264
echo   React Admin:       http://localhost:5173
echo   MVC User Portal:   http://localhost:5066
echo.
echo ================================================================
echo  TEST ACCOUNTS
echo ================================================================
echo.
echo   React Admin:  admin@ngo.org / Admin123!
echo   MVC Portal:   test.user@example.com / Test123!
echo   ECPay Card:   4311-9511-1111-1111, 12/25, 222
echo.
echo ================================================================
echo.
echo  Total windows: 4 (3 services + this control window)
echo.
echo  To stop all: Close all windows
echo.
echo ================================================================
echo.
echo  [Q] Quit (keep services running)
echo.

:input_loop
set /p "choice=Enter choice: "
if /i "%choice%"=="Q" goto end
goto input_loop

:end
echo.
echo  Services are still running in other windows.
echo  Close them manually when done.
echo.
pause
