@echo off
chcp 65001 >nul
color 0C
echo.
echo ════════════════════════════════════════════════════════════════
echo  重新啟動展示系統
echo ════════════════════════════════════════════════════════════════
echo.
echo 這個腳本會清理所有舊的進程並重新啟動系統
echo.
pause

echo.
echo [1/4] 停止所有舊的進程...
echo.
taskkill /F /IM node.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM ngrok.exe >nul 2>&1
timeout /t 3 >nul

echo ✓ 已停止所有進程
echo.

echo [2/4] 啟動後端 WebAPI...
cd /d "%~dp0..\NGO-Admin-System-WebAPI"
start "【NGO】後端 WebAPI" cmd /k "dotnet run"
timeout /t 15 >nul
echo ✓ 後端啟動完成
echo.

echo [3/4] 啟動前端 React...
cd /d "%~dp0..\NGO-Admin-System"
start "【NGO】前端 React" cmd /k "npm run dev"
timeout /t 10 >nul
echo ✓ 前端啟動完成
echo.

echo [4/4] 啟動 ngrok tunnels...
start "【NGO】ngrok 後端" cmd /k "echo ngrok 後端 tunnel 正在運行... && echo. && ngrok http 5264 --log=stdout"
timeout /t 3 >nul
start "【NGO】ngrok 前端" cmd /k "echo ngrok 前端 tunnel 正在運行... && echo. && ngrok http 5173 --log=stdout"
timeout /t 5 >nul
echo ✓ ngrok tunnels 啟動完成
echo.

cls
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║                   ✓ 系統啟動完成！                              ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo 正在獲取公開網址...
timeout /t 3 >nul

curl -s http://localhost:4040/api/tunnels | findstr "public_url"

echo.
echo ════════════════════════════════════════════════════════════════
echo  請前往 http://localhost:4040 查看完整網址
echo ════════════════════════════════════════════════════════════════
echo.
echo 按任意鍵開啟 ngrok 管理介面...
pause >nul

start http://localhost:4040

echo.
echo 展示系統正在運行中...
echo.
pause
