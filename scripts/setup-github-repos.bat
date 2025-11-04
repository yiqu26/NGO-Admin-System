@echo off
echo ========================================
echo NGO 專案 - 建立 GitHub Repositories
echo ========================================
echo.

REM 請在這裡輸入你的 GitHub 使用者名稱
set GITHUB_USERNAME=yiqu26

echo GitHub 使用者名稱: %GITHUB_USERNAME%
echo.
echo 這個腳本會：
echo 1. 初始化 Git repositories
echo 2. 添加所有檔案到 Git
echo 3. 建立初始 commit
echo 4. 設定 GitHub remote
echo.
echo 注意：你需要先在 GitHub 上建立兩個空的 repositories：
echo   - NGO-Admin-System (前端)
echo   - NGO-Admin-System-WebAPI (後端)
echo.
pause

echo.
echo ========================================
echo 處理前端專案 (React)
echo ========================================
cd /d "%~dp0..\NGO-Admin-System"

echo [1/4] 初始化 Git...
git init

echo [2/4] 添加檔案到 Git...
git add .

echo [3/4] 建立初始 commit...
git commit -m "Initial commit - NGO Admin System Frontend"

echo [4/4] 設定 GitHub remote...
git branch -M main
git remote add origin https://github.com/%GITHUB_USERNAME%/NGO-Admin-System.git

echo.
echo 前端專案準備完成！
echo 請在 GitHub 上建立 repository: NGO-Admin-System
echo 然後執行: git push -u origin main
echo.

echo ========================================
echo 處理後端專案 (WebAPI)
echo ========================================
cd /d "%~dp0..\NGO-Admin-System-WebAPI"

echo [1/4] 初始化 Git...
git init

echo [2/4] 添加檔案到 Git...
git add .

echo [3/4] 建立初始 commit...
git commit -m "Initial commit - NGO Admin System WebAPI Backend"

echo [4/4] 設定 GitHub remote...
git branch -M main
git remote add origin https://github.com/%GITHUB_USERNAME%/NGO-Admin-System-WebAPI.git

echo.
echo 後端專案準備完成！
echo 請在 GitHub 上建立 repository: NGO-Admin-System-WebAPI
echo 然後執行: git push -u origin main
echo.

echo ========================================
echo 完成！
echo ========================================
echo.
echo 下一步：
echo 1. 前往 https://github.com/new
echo 2. 建立 repository: NGO-Admin-System (前端)
echo 3. 建立 repository: NGO-Admin-System-WebAPI (後端)
echo 4. 在各專案目錄執行: git push -u origin main
echo.
pause
