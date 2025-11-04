@echo off
chcp 65001 >nul
color 0E
title 設定 ngrok 配置文件
cls

echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo              ngrok 配置文件設定工具
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo  此腳本會幫您設定 ngrok 配置文件，讓您可以同時運行多個 tunnels
echo.
echo  免費版 ngrok 限制：
echo    ✗ 不能同時運行多個獨立的 ngrok 進程
echo    ✓ 可以在一個進程中運行多個 tunnels (使用配置文件)
echo.
echo ════════════════════════════════════════════════════════════════
echo.

REM 檢查是否已經有配置
echo [步驟 1/3] 檢查現有配置...
echo.

ngrok config check >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✓ 找到現有的 ngrok 配置
) else (
    echo ⚠ 未找到有效的 ngrok 配置
)

echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo [步驟 2/3] 添加 tunnel 配置...
echo.

REM 添加 backend tunnel
echo 正在配置後端 tunnel (port 5264)...
ngrok config add-tunnel backend --addr 5264 --proto http
echo.

REM 添加 frontend tunnel
echo 正在配置前端 tunnel (port 5173)...
ngrok config add-tunnel frontend --addr 5173 --proto http
echo.

echo ✓ Tunnel 配置完成
echo.

echo ════════════════════════════════════════════════════════════════
echo.
echo [步驟 3/3] 驗證配置...
echo.

ngrok config check
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ════════════════════════════════════════════════════════════════
    echo.
    echo  ✓ ngrok 配置成功！
    echo.
    echo ════════════════════════════════════════════════════════════════
    echo.
    echo  您現在可以使用：
    echo    啟動展示系統-修復版.bat
    echo.
    echo  該腳本會使用 "ngrok start --all" 同時啟動兩個 tunnels
    echo.
    echo ════════════════════════════════════════════════════════════════
) else (
    echo.
    echo ════════════════════════════════════════════════════════════════
    echo.
    echo  ❌ 配置驗證失敗
    echo.
    echo ════════════════════════════════════════════════════════════════
    echo.
    echo  可能原因：
    echo    1. 您還沒有設定 ngrok authtoken
    echo    2. ngrok 版本過舊
    echo.
    echo  解決方法：
    echo    1. 前往 https://dashboard.ngrok.com/get-started/your-authtoken
    echo    2. 複製您的 authtoken
    echo    3. 執行命令：ngrok config add-authtoken YOUR_TOKEN
    echo.
    echo ════════════════════════════════════════════════════════════════
)

echo.
pause
