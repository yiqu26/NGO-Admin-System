@echo off
chcp 65001 >nul
color 0B
title NGO 展示系統啟動器 (修復版)
cls

echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo              NGO 管理系統 - 展示模式啟動器
echo.
echo              GitHub: github.com/yiqu26/NGO-Admin-System
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo  此腳本會自動啟動完整的展示系統：
echo.
echo    [1] 清理舊的進程
echo    [2] 啟動後端 WebAPI (ASP.NET Core)
echo    [3] 啟動前端 React (Vite)
echo    [4] 啟動 ngrok (前端 + 後端，使用單一 session)
echo    [5] 顯示公開網址
echo.
echo  ════════════════════════════════════════════════════════════════
echo.
echo  重要提示：
echo    • 展示期間請保持電腦開機
echo    • 每次重啟，ngrok 網址會改變
echo    • 第一次訪問 ngrok 網址會有警告頁，點擊 Visit Site 繼續
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
echo  [1/5] 清理舊的進程...
echo ════════════════════════════════════════════════════════════════
echo.

taskkill /F /IM node.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM ngrok.exe >nul 2>&1

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
echo  [2/5] 啟動後端 WebAPI...
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
echo  [3/5] 啟動前端 React...
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
REM 第 4 步：啟動 ngrok tunnels (使用單一 session)
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  [4/5] 啟動 ngrok tunnels (前端 + 後端)...
echo ════════════════════════════════════════════════════════════════
echo.

REM 使用 ngrok 配置文件啟動多個 tunnels
start "【NGO】ngrok Tunnels (前端 + 後端)" cmd /k "echo ngrok tunnels 正在運行... && echo 管理介面: http://localhost:4040 && echo. && ngrok start --all"

echo  正在啟動 ngrok 服務...
timeout /t 8 /nobreak >nul
echo  ✓ ngrok tunnels 啟動完成
echo.
timeout /t 1 /nobreak >nul

REM ================================================================
REM 第 5 步：顯示公開網址
REM ================================================================
cls
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo                    ✓ 系統啟動成功！
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo  [5/5] 正在獲取公開網址...
echo.

timeout /t 2 /nobreak >nul

echo ════════════════════════════════════════════════════════════════
echo  📋 本地服務地址
echo ════════════════════════════════════════════════════════════════
echo.
echo   後端 API:    http://localhost:5264
echo   前端 UI:     http://localhost:5173
echo   ngrok 管理:  http://localhost:4040
echo.
echo ════════════════════════════════════════════════════════════════
echo  🌐 公開網址（給面試官）
echo ════════════════════════════════════════════════════════════════
echo.
echo   請開啟瀏覽器前往: http://localhost:4040
echo   查看完整的 ngrok 公開網址
echo.
echo   找到：
echo     • frontend (5173) 的 HTTPS 網址 ← 這個給面試官
echo     • backend (5264) 的 HTTPS 網址  ← 後端 API
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
echo   1. 按任意鍵開啟 ngrok 管理介面
echo   2. 複製前端網址並測試登入
echo   3. 展示時保持此視窗開啟
echo.
echo ════════════════════════════════════════════════════════════════
echo.

pause

REM 開啟 ngrok 管理介面
start http://localhost:4040

echo.
echo  所有服務正在運行中...
echo  若要停止，請關閉所有命令提示字元視窗
echo.
echo  專案 GitHub: https://github.com/yiqu26/NGO-Admin-System
echo.
pause
