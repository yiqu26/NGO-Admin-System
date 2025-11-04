@echo off
chcp 65001 >nul
color 0A
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║           NGO 管理系統 - 展示模式啟動程式                      ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo 此程式會自動啟動：
echo   [1] 後端 WebAPI (http://localhost:5264)
echo   [2] 前端 React   (http://localhost:5173)
echo   [3] ngrok 後端   (公開 API 網址)
echo   [4] ngrok 前端   (公開展示網址)
echo.
echo 啟動後，你會得到兩個公開網址：
echo   • 前端網址 - 放在簡歷上，給面試官看
echo   • 後端網址 - 供前端呼叫 API
echo.
echo 按任意鍵開始啟動...
pause >nul

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 1/4：啟動後端 WebAPI
echo ════════════════════════════════════════════════════════════════
echo.

cd /d "%~dp0..\NGO-Admin-System-WebAPI"
start "【NGO】後端 WebAPI" cmd /k "echo 後端 WebAPI 正在運行... && echo. && dotnet run"

echo ✓ 後端 WebAPI 啟動中...
echo   (運行在 http://localhost:5264)
echo.
echo 等待後端完全啟動...
timeout /t 15 /nobreak >nul

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 2/4：啟動前端 React
echo ════════════════════════════════════════════════════════════════
echo.

cd /d "%~dp0..\NGO-Admin-System"
start "【NGO】前端 React" cmd /k "echo 前端 React 正在運行... && echo. && npm run dev"

echo ✓ 前端 React 啟動中...
echo   (運行在 http://localhost:5173)
echo.
echo 等待前端完全啟動...
timeout /t 10 /nobreak >nul

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 3/4：啟動 ngrok 後端 tunnel
echo ════════════════════════════════════════════════════════════════
echo.

start "【NGO】ngrok 後端" cmd /k "echo ngrok 後端 tunnel 正在運行... && echo. && ngrok http 5264 --log=stdout"

echo ✓ ngrok 後端 tunnel 啟動中...
echo.
echo 等待 ngrok 完全啟動...
timeout /t 5 /nobreak >nul

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 4/4：啟動 ngrok 前端 tunnel
echo ════════════════════════════════════════════════════════════════
echo.

start "【NGO】ngrok 前端" cmd /k "echo ngrok 前端 tunnel 正在運行... && echo. && ngrok http 5173 --log=stdout"

echo ✓ ngrok 前端 tunnel 啟動中...
echo.
echo 等待 ngrok 完全啟動...
timeout /t 5 /nobreak >nul

cls
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║                      正在獲取公開網址...                        ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

timeout /t 3 /nobreak >nul

echo 查詢 ngrok API...
curl -s http://localhost:4040/api/tunnels > %TEMP%\ngrok.json 2>nul

echo.
echo ════════════════════════════════════════════════════════════════
echo  🎉 所有服務啟動完成！
echo ════════════════════════════════════════════════════════════════
echo.

REM 解析 ngrok URL (簡單版本)
for /f "tokens=*" %%i in ('curl -s http://localhost:4040/api/tunnels ^| findstr "public_url"') do (
    echo %%i
)

echo.
echo ════════════════════════════════════════════════════════════════
echo  📋 服務狀態
echo ════════════════════════════════════════════════════════════════
echo.
echo  [本地服務]
echo    • 後端 API:  http://localhost:5264
echo    • 前端 UI:   http://localhost:5173
echo    • ngrok 管理: http://localhost:4040
echo.
echo  [公開網址]
echo    → 請到 http://localhost:4040 查看完整的 ngrok 網址
echo    → 前端網址：找 5173 的 HTTPS 網址
echo    → 後端網址：找 5264 的 HTTPS 網址
echo.
echo ════════════════════════════════════════════════════════════════
echo  📌 重要提示
echo ════════════════════════════════════════════════════════════════
echo.
echo  1. 前端網址可以放在簡歷上給面試官
echo  2. 展示時需要保持電腦開機
echo  3. 每次重啟，ngrok 網址會改變（免費版限制）
echo  4. 關閉時，請關閉所有開啟的視窗
echo.
echo  [測試連線]
echo    前端：用瀏覽器開啟 ngrok 前端網址
echo    登入：admin@ngo.org / Admin123!
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo 按任意鍵開啟 ngrok 管理介面...
pause >nul

start http://localhost:4040

echo.
echo 所有服務正在背景運行中...
echo 請勿關閉此視窗！
echo.
echo 若要停止所有服務，請關閉所有開啟的命令提示字元視窗。
echo.
pause
