@echo off
echo ========================================
echo NGO 專案 - 啟動 ngrok SQL Server Tunnel
echo ========================================
echo.
echo 這個腳本會建立 SQL Server 的 ngrok tunnel
echo 讓 Render 上的 WebAPI 可以連接到本地資料庫
echo.
echo 請確保：
echo 1. 已安裝 ngrok
echo 2. 已設定 ngrok authtoken
echo 3. SQL Server 正在運行
echo.
pause

cd /d %~dp0
ngrok tcp 1433 --log=stdout

pause
