@echo off
chcp 65001 >nul
color 0B
title NGO 展示系統啟動器 (Cloudflare Tunnel)
cls

echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║         NGO 管理系統 - 展示模式啟動器 (Cloudflare)             ║
echo ║                                                                ║
echo ║         使用 Cloudflare Tunnel 取代 ngrok                       ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo  此腳本會自動啟動完整的展示系統：
echo.
echo    [1] 清理舊的進程
echo    [2] 啟動後端 WebAPI (ASP.NET Core)
echo    [3] 啟動前端 React (Vite)
echo    [4] 啟動 Cloudflare Tunnel (前端)
echo    [5] 啟動 Cloudflare Tunnel (後端)
echo    [6] 顯示公開網址
echo.
echo  ════════════════════════════════════════════════════════════════
echo.
echo  優點：
echo    • 比 ngrok 更穩定
echo    • 沒有 ngrok 的警告頁
echo    • 完全免費
echo    • 網址更持久
echo.
echo  ════════════════════════════════════════════════════════════════
echo.
pause

REM ================================================================
REM 第 1 步：清理舊的進程
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [1/6] 清理舊的進程...
echo ════════════════════════════════════════════════════════════════
echo.

taskkill /F /IM node.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM cloudflared.exe >nul 2>&1

timeout /t 2 /nobreak >nul
echo  ✓ 已清理所有舊進程
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 2 步：啟動後端 WebAPI
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [2/6] 啟動後端 WebAPI...
echo ════════════════════════════════════════════════════════════════
echo.

cd /d "%~dp0..\NGO-Admin-System-WebAPI"
start "【NGO】後端 WebAPI (localhost:5264)" cmd /k "echo 後端 WebAPI 正在運行... && echo 運行在: http://localhost:5264 && echo. && dotnet run"

echo  正在啟動後端服務...
timeout /t 15 /nobreak >nul
echo  ✓ 後端 WebAPI 啟動完成
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 3 步：啟動前端 React
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [3/6] 啟動前端 React...
echo ════════════════════════════════════════════════════════════════
echo.

cd /d "%~dp0..\NGO-Admin-System"
start "【NGO】前端 React (localhost:5173)" cmd /k "echo 前端 React 正在運行... && echo 運行在: http://localhost:5173 && echo. && npm run dev"

echo  正在啟動前端服務...
timeout /t 10 /nobreak >nul
echo  ✓ 前端 React 啟動完成
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 4 步：啟動 Cloudflare Tunnel (後端)
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [4/6] 啟動 Cloudflare Tunnel (後端 API)...
echo ════════════════════════════════════════════════════════════════
echo.

start "【NGO】Cloudflare Tunnel - 後端 API (5264)" cmd /k "echo Cloudflare Tunnel 後端正在運行... && echo 公開網址將在視窗中顯示 && echo. && cloudflared tunnel --url http://localhost:5264"

echo  正在啟動 Cloudflare Tunnel（後端）...
timeout /t 5 /nobreak >nul
echo  ✓ 後端 Tunnel 啟動完成
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 5 步：啟動 Cloudflare Tunnel (前端)
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [5/6] 啟動 Cloudflare Tunnel (前端 React)...
echo ════════════════════════════════════════════════════════════════
echo.

start "【NGO】Cloudflare Tunnel - 前端 React (5173)" cmd /k "echo Cloudflare Tunnel 前端正在運行... && echo 公開網址將在視窗中顯示 && echo. && cloudflared tunnel --url http://localhost:5173"

echo  正在啟動 Cloudflare Tunnel（前端）...
timeout /t 5 /nobreak >nul
echo  ✓ 前端 Tunnel 啟動完成
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 6 步：顯示說明
REM ================================================================
cls
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║                    ✓ 系統啟動成功！                             ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo  [6/6] 查看公開網址
echo.
echo ════════════════════════════════════════════════════════════════
echo  📋 本地服務地址
echo ════════════════════════════════════════════════════════════════
echo.
echo   後端 API:    http://localhost:5264
echo   前端 UI:     http://localhost:5173
echo.
echo ════════════════════════════════════════════════════════════════
echo  🌐 公開網址（給面試官）
echo ════════════════════════════════════════════════════════════════
echo.
echo   請查看 Cloudflare Tunnel 視窗中的公開網址
echo.
echo   找到：
echo     • 【NGO】Cloudflare Tunnel - 前端 React (5173)
echo       └─ 視窗中會顯示類似：
echo          https://xxxxx-xxxxx-xxxx.trycloudflare.com
echo          這個網址給面試官 ← 前端公開網址
echo.
echo     • 【NGO】Cloudflare Tunnel - 後端 API (5264)
echo       └─ 視窗中會顯示類似：
echo          https://xxxxx-xxxxx-xxxx.trycloudflare.com
echo          這個是後端 API 網址
echo.
echo   注意：
echo   • 這些網址在系統重啟前都是固定的
echo   • 比 ngrok 更穩定，沒有警告頁
echo   • 完全免費
echo.
echo ════════════════════════════════════════════════════════════════
echo  🔐 測試帳號
echo ════════════════════════════════════════════════════════════════
echo.
echo   管理員：admin@ngo.org / Admin123!
echo   督導：  supervisor@ngo.org / Super123!
echo   員工：  staff@ngo.org / Staff123!
echo.
echo ════════════════════════════════════════════════════════════════
echo  📝 下一步
echo ════════════════════════════════════════════════════════════════
echo.
echo   1. 複製前端 Cloudflare Tunnel 視窗中的網址
echo   2. 在瀏覽器中開啟該網址
echo   3. 使用測試帳號登入
echo   4. 展示時保持所有視窗開啟
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo  所有服務正在運行中...
echo  若要停止，請關閉所有命令提示字元視窗
echo.
echo  專案 GitHub: https://github.com/yiqu26/NGO-Admin-System
echo.
pause
