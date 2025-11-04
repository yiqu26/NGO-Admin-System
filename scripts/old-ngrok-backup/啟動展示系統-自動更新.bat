@echo off
chcp 65001 >nul
color 0A
cls

echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║      NGO 管理系統 - 智能展示模式（自動更新 API 網址）          ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo 本腳本會自動：
echo   [1] 啟動後端 WebAPI
echo   [2] 啟動 ngrok 並取得公開網址
echo   [3] 自動更新前端環境變數
echo   [4] 啟動前端 React
echo.
echo 按任意鍵開始...
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
echo  步驟 2/4：啟動 ngrok 並取得公開網址
echo ════════════════════════════════════════════════════════════════
echo.

REM 啟動 ngrok
start "【NGO】ngrok 後端" cmd /k "echo ngrok 正在運行... && echo. && ngrok http 5264 --log=stdout"

echo ✓ ngrok 啟動中...
echo.
echo 等待 ngrok 完全啟動...
timeout /t 8 /nobreak >nul

REM 取得 ngrok 公開網址
echo.
echo 正在取得 ngrok 公開網址...
powershell -Command "$response = Invoke-RestMethod -Uri 'http://localhost:4040/api/tunnels' -ErrorAction SilentlyContinue; if ($response) { $url = $response.tunnels[0].public_url; Write-Host $url; $url | Out-File -FilePath '%TEMP%\ngrok_url.txt' -Encoding UTF8 } else { Write-Host 'ERROR: 無法取得 ngrok 網址'; }"

REM 讀取 ngrok 網址
set /p NGROK_URL=<"%TEMP%\ngrok_url.txt"

if "%NGROK_URL%"=="" (
    echo.
    echo ❌ 錯誤：無法取得 ngrok 網址
    echo.
    echo 可能原因：
    echo   1. ngrok 還沒啟動完成
    echo   2. ngrok 設定有問題
    echo.
    echo 請手動檢查 http://localhost:4040
    pause
    exit /b 1
)

echo.
echo ✓ 已取得 ngrok 公開網址：
echo   %NGROK_URL%
echo.

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 3/4：更新前端環境變數
echo ════════════════════════════════════════════════════════════════
echo.

REM 更新 .env.development
set ENV_FILE=%~dp0..\NGO-Admin-System\.env.development
set API_URL=%NGROK_URL%/api

echo 正在更新環境變數檔案...
echo 檔案：NGO-Admin-System\.env.development
echo 新的 API 網址：%API_URL%
echo.

REM 使用 PowerShell 更新檔案
powershell -Command "(Get-Content '%ENV_FILE%') -replace 'VITE_API_BASE_URL=.*', 'VITE_API_BASE_URL=%API_URL%' | Set-Content '%ENV_FILE%' -Encoding UTF8NoBOM"

echo ✓ 環境變數已更新
echo.
timeout /t 2 /nobreak >nul

cls
echo.
echo ════════════════════════════════════════════════════════════════
echo  步驟 4/4：啟動前端 React
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
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║                      🎉 所有服務啟動完成！                      ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
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
echo    • 後端 API:  %NGROK_URL%
echo    • 前端環境變數已自動更新為上述網址
echo.
echo ════════════════════════════════════════════════════════════════
echo  📌 重要提示
echo ════════════════════════════════════════════════════════════════
echo.
echo  1. ✓ 前端環境變數已自動更新
echo  2. ✓ 所有服務已啟動
echo  3. → 請稍等 10-20 秒讓前端完全載入
echo  4. → 前端會自動開啟瀏覽器（或手動開啟 http://localhost:5173）
echo.
echo  [測試連線]
echo    登入帳號：admin@ngo.org
echo    密碼：Admin123!
echo.
echo  [注意事項]
echo    • 展示期間請保持所有視窗開啟
echo    • 不要關閉此視窗
echo    • ngrok 免費版網址每次重啟會改變
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo 按任意鍵開啟 ngrok 管理介面（查看詳細資訊）...
pause >nul

start http://localhost:4040

echo.
echo 所有服務正在背景運行中...
echo 請勿關閉此視窗！
echo.
echo 若要停止所有服務，請關閉所有開啟的命令提示字元視窗。
echo.
pause
