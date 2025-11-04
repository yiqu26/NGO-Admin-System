# 🔧 ngrok 展示模式問題排查指南

> 解決使用 ngrok 時資料無法載入的問題

**最後更新**：2025-11-01

---

## 🚨 問題描述

使用 ngrok 展示系統時，前端可以開啟，但是：
- ❌ Dashboard 資料無法載入
- ❌ 登入可能失敗
- ❌ 個案/活動/物資資料顯示為空
- ❌ 瀏覽器 Console 顯示 API 連線錯誤

---

## 🔍 問題原因

### 主要原因：ngrok 網址已過期

**ngrok 免費版的限制**：
- 每次重啟 ngrok 會產生新的網址
- 舊的網址會立即失效
- 前端的環境變數仍指向舊的 ngrok 網址

**您目前的設定**：
```
檔案：NGO-Admin-System\.env.development
第7行：VITE_API_BASE_URL=https://20d5d47def25.ngrok-free.app/api
                                    ^^^^^^^^^^^^^^^^
                                    這是舊的 ngrok 網址！
```

---

## ✅ 解決方案

### 方案 A：手動更新（推薦，完整控制）

#### Step 1：啟動後端和 ngrok

開啟終端機 1：
```bash
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run
```

等待後端啟動完成（看到 "Now listening on: http://localhost:5264"）

開啟終端機 2：
```bash
ngrok http 5264
```

#### Step 2：取得新的 ngrok 網址

在終端機 2 中，找到類似這樣的訊息：
```
Forwarding  https://abcd1234efgh.ngrok-free.app -> http://localhost:5264
                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    複製這個網址（不含後面的部分）
```

或開啟瀏覽器訪問：http://localhost:4040
- 查看 "Forwarding" 欄位
- 複製 HTTPS 網址

#### Step 3：更新前端環境變數

1. 開啟檔案：`NGO-Admin-System\.env.development`

2. 修改第 7 行：
```env
# 將舊的網址
VITE_API_BASE_URL=https://20d5d47def25.ngrok-free.app/api

# 改成新的網址（記得加 /api）
VITE_API_BASE_URL=https://abcd1234efgh.ngrok-free.app/api
```

3. **重要**：網址後面要加 `/api`！

4. 儲存檔案

#### Step 4：啟動前端

**如果前端還沒啟動**：
```bash
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev
```

**如果前端已經在運行**：
1. 在前端終端機按 `Ctrl + C` 停止
2. 重新執行 `npm run dev`

**為什麼要重啟？**
- Vite 需要重啟才能讀取新的環境變數
- 僅重新整理瀏覽器是不夠的

#### Step 5：驗證

1. 開啟瀏覽器：http://localhost:5173

2. 按 F12 開啟開發者工具

3. 切換到 "Network" 標籤

4. 嘗試登入或載入資料

5. 檢查 API 請求：
   - ✅ 請求網址應該是新的 ngrok 網址
   - ✅ 狀態碼應該是 200 或 201
   - ❌ 如果是 ERR_NAME_NOT_RESOLVED，網址錯誤
   - ❌ 如果是 CORS error，檢查後端是否運行

---

### 方案 B：使用自動化腳本（快速但需要測試）

我可以幫您創建一個自動化腳本，執行以下步驟：
1. 啟動後端
2. 啟動 ngrok
3. 自動取得新網址
4. 自動更新 .env.development
5. 啟動前端

**要我創建這個腳本嗎？**

---

### 方案 C：使用本地網址（測試用）

如果只是要測試系統功能（不需要公開網址）：

1. 修改 `.env.development`：
```env
VITE_API_BASE_URL=http://localhost:5264/api
```

2. 重啟前端

3. 這樣前端就會直接連本地後端，不需要 ngrok

**限制**：只有您的電腦可以訪問，無法分享給他人

---

## 🎯 完整啟動流程（推薦）

### 每次展示前

```
1. 啟動後端 WebAPI
   ↓
2. 啟動 ngrok
   ↓
3. 取得 ngrok 新網址
   ↓
4. 更新 .env.development
   ↓
5. 啟動（或重啟）前端
   ↓
6. 驗證 API 連線
   ↓
7. 開始展示！
```

### 詳細指令

```bash
# 終端機 1 - 後端
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run

# 終端機 2 - ngrok（等後端啟動後再執行）
ngrok http 5264

# 瀏覽器
# 開啟 http://localhost:4040
# 複製 ngrok HTTPS 網址

# 編輯器
# 開啟 NGO-Admin-System\.env.development
# 更新 VITE_API_BASE_URL=https://新的ngrok網址/api
# 儲存

# 終端機 3 - 前端
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev

# 瀏覽器
# 開啟 http://localhost:5173
# 測試登入和資料載入
```

---

## 🔍 詳細問題診斷

### 檢查清單

#### ✅ 1. 後端是否正常運行？

**測試**：
```bash
# 在瀏覽器開啟
http://localhost:5264

# 應該看到
NGO API 運作正常 - 2025-11-01 14:30:00
```

**如果失敗**：
- 檢查 SQL Server 是否啟動
- 檢查 port 5264 是否被占用
- 查看後端終端機的錯誤訊息

---

#### ✅ 2. ngrok 是否正常運行？

**測試**：
```bash
# 在瀏覽器開啟
http://localhost:4040

# 應該看到
ngrok 管理介面，顯示 tunnel 資訊
```

**檢查**：
- Status 應該是 "online"
- Forwarding 應該顯示 HTTPS 網址

**如果失敗**：
- 檢查 ngrok 是否已安裝
- 檢查 authtoken 是否設定
- 重新執行 `ngrok http 5264`

---

#### ✅ 3. ngrok 可以連到後端嗎？

**測試**：
```bash
# 複製 ngrok 給的 HTTPS 網址
# 在瀏覽器開啟（不加 /api）
https://abcd1234efgh.ngrok-free.app

# 應該看到
NGO API 運作正常 - 2025-11-01 14:30:00
```

**如果看到 ngrok 警告頁面**：
- 點擊 "Visit Site" 繼續
- 這是 ngrok 免費版的正常提示

**如果失敗**：
- 檢查後端是否正常運行
- 檢查 ngrok 的 port 是否正確（應該是 5264）

---

#### ✅ 4. 前端環境變數是否正確？

**檢查**：
```bash
# 開啟檔案
NGO-Admin-System\.env.development

# 檢查第 7 行
VITE_API_BASE_URL=https://你的ngrok網址/api
                                      ^^^^
                                      必須有 /api
```

**常見錯誤**：
```env
# ❌ 錯誤：沒有 /api
VITE_API_BASE_URL=https://abcd1234efgh.ngrok-free.app

# ❌ 錯誤：使用舊的網址
VITE_API_BASE_URL=https://20d5d47def25.ngrok-free.app/api

# ❌ 錯誤：使用 http 而不是 https
VITE_API_BASE_URL=http://abcd1234efgh.ngrok-free.app/api

# ✅ 正確
VITE_API_BASE_URL=https://abcd1234efgh.ngrok-free.app/api
```

---

#### ✅ 5. 前端是否已重啟？

**重要**：修改 .env 文件後，**必須重啟前端**！

**重啟步驟**：
1. 在前端終端機按 `Ctrl + C`
2. 等待完全停止
3. 重新執行 `npm run dev`

**僅重新整理瀏覽器是不夠的！**

---

#### ✅ 6. 瀏覽器 Console 顯示什麼錯誤？

**按 F12 開啟開發者工具**，切換到 "Console" 標籤

**常見錯誤 1**：
```
ERR_NAME_NOT_RESOLVED
```
**原因**：ngrok 網址錯誤或已過期
**解決**：更新 .env.development 的網址

---

**常見錯誤 2**：
```
CORS policy: No 'Access-Control-Allow-Origin' header
```
**原因**：後端未啟動或 CORS 設定問題
**解決**：
- 確認後端正在運行
- 檢查後端 Program.cs 的 CORS 設定

---

**常見錯誤 3**：
```
Failed to fetch / Network error
```
**原因**：前端無法連接到後端
**解決**：
- 檢查 ngrok 是否正常運行
- 檢查網址是否正確
- 測試 ngrok 網址是否可以在瀏覽器直接訪問

---

**常見錯誤 4**：
```
401 Unauthorized
```
**原因**：JWT Token 問題
**解決**：
- 重新登入
- 檢查測試帳號是否正確
- 檢查資料庫是否有測試帳號

---

## 💡 避免問題的最佳實踐

### 1. 使用 ngrok 付費版（推薦）

**優點**：
- ✅ 固定網址，不會改變
- ✅ 不需要每次更新 .env
- ✅ 更穩定的連線

**費用**：$8/月

**設定**：
```bash
# 升級後設定固定網域
ngrok http 5264 --domain=your-fixed-domain.ngrok-free.app
```

---

### 2. 創建環境變數範本

**步驟**：
1. 複製 `.env.development` 為 `.env.development.template`
2. 在範本中使用佔位符：
```env
VITE_API_BASE_URL=https://YOUR_NGROK_URL_HERE/api
```
3. 每次展示前從範本複製並填入新網址

---

### 3. 使用腳本自動化

創建一個批次檔自動執行所有步驟（我可以幫您創建）

---

### 4. 部署到雲端（長期方案）

**好處**：
- ✅ 固定網址
- ✅ 不需要保持電腦開機
- ✅ 更專業的展示

**參考**：
- [部署指南-免費方案](部署指南-免費方案.md)

---

## 📝 快速參考卡

### 每次展示前必做

```
□ 啟動後端
□ 啟動 ngrok
□ 取得新網址（http://localhost:4040）
□ 更新 .env.development
□ 重啟前端（不是重新整理瀏覽器！）
□ 測試 API 連線
```

### 驗證清單

```
□ http://localhost:5264 可以訪問
□ http://localhost:4040 顯示 ngrok 介面
□ ngrok HTTPS 網址可以訪問
□ .env.development 的網址是最新的
□ 前端已經重啟
□ 瀏覽器 Console 沒有錯誤
```

---

## 🆘 仍然無法解決？

### 完整除錯步驟

1. **停止所有服務**
   - 關閉所有終端機
   - 關閉瀏覽器

2. **按順序重新啟動**
   ```bash
   # 1. 確認 SQL Server 運行中
   # 2. 啟動後端
   cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
   dotnet run

   # 3. 等待後端完全啟動
   # 4. 啟動 ngrok
   ngrok http 5264

   # 5. 取得網址並更新 .env.development
   # 6. 啟動前端
   cd C:\Users\lanli\source\repos\NGO-Admin-System
   npm run dev
   ```

3. **測試每個步驟**
   - 每個服務啟動後都測試是否正常
   - 記錄任何錯誤訊息

4. **提供以下資訊**
   - 後端終端機的錯誤訊息
   - 前端終端機的錯誤訊息
   - 瀏覽器 Console 的錯誤
   - .env.development 的內容（第7行）
   - ngrok 顯示的網址

---

## 📞 相關文檔

- [展示系統使用說明](展示系統使用說明.md)
- [本地開發環境啟動指南](本地開發環境啟動指南.md)
- [部署指南-免費方案](部署指南-免費方案.md)

---

**最後更新**：2025-11-01
**狀態**：✅ 已驗證有效
