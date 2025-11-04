# ngrok 免費版限制修復說明

## ❌ 問題

您遇到的錯誤：
```
ERR_NGROK_108: Your account is limited to 1 simultaneous ngrok agent sessions.
```

**原因：** ngrok 免費版只能同時運行 **1 個 agent session**，但舊腳本嘗試啟動了 2 個獨立的 ngrok 進程。

## ✅ 解決方案

使用 **ngrok 配置文件** 在單一 session 中運行多個 tunnels（免費版支持！）

---

## 🚀 使用步驟

### 第一次使用（一次性設定）

**1. 執行設定腳本**
```batch
雙擊執行：設定ngrok配置.bat
```

這個腳本會：
- 檢查現有的 ngrok 配置
- 添加 backend tunnel (port 5264)
- 添加 frontend tunnel (port 5173)
- 驗證配置是否正確

**2. 如果出現 authtoken 錯誤**

請執行以下步驟：
1. 前往 https://dashboard.ngrok.com/get-started/your-authtoken
2. 登入您的 ngrok 帳號
3. 複製您的 authtoken
4. 執行命令：
   ```batch
   ngrok config add-authtoken YOUR_AUTHTOKEN_HERE
   ```
5. 重新執行 `設定ngrok配置.bat`

---

### 日常使用

設定完成後，每次啟動系統時：

```batch
雙擊執行：啟動展示系統-修復版.bat
```

這個腳本會：
1. ✓ 清理舊進程
2. ✓ 啟動後端 WebAPI (localhost:5264)
3. ✓ 啟動前端 React (localhost:5173)
4. ✓ **使用單一 ngrok session 啟動兩個 tunnels**
5. ✓ 顯示公開網址

---

## 📊 對比

### 舊方案（會失敗）
```batch
# 啟動兩個獨立的 ngrok 進程 ❌
start ngrok http 5264
start ngrok http 5173
```
**結果：** ERR_NGROK_108 錯誤

### 新方案（成功）
```batch
# 使用配置文件在一個 session 中運行多個 tunnels ✓
ngrok start --all
```
**結果：** 成功運行兩個 tunnels

---

## 🔧 技術細節

ngrok 配置文件位置：
```
C:\Users\lanli\AppData\Local\ngrok\ngrok.yml
```

配置內容：
```yaml
version: 2
authtoken: YOUR_AUTHTOKEN
tunnels:
  backend:
    proto: http
    addr: 5264
  frontend:
    proto: http
    addr: 5173
```

---

## 📝 常見問題

**Q: 為什麼會有這個限制？**
A: ngrok 免費版限制同時運行的 agent sessions 數量為 1，但允許在單一 session 中運行多個 tunnels。

**Q: 需要升級到付費版嗎？**
A: 不需要！使用配置文件的方式完全免費。

**Q: 如何查看所有 tunnels 的網址？**
A: 開啟瀏覽器前往 http://localhost:4040，可以看到所有 tunnels 的詳細信息。

**Q: ngrok 網址每次都會改變嗎？**
A: 是的，免費版每次重啟會產生新的隨機網址。付費版可以使用固定網址。

---

## 📁 相關文件

- `設定ngrok配置.bat` - 一次性設定腳本
- `啟動展示系統-修復版.bat` - 日常使用的啟動腳本
- `ngrok-config.yml` - 範例配置文件（僅供參考）

---

## 🆘 需要幫助？

如果還有問題，請檢查：
1. ngrok 版本是否為最新版本：`ngrok version`
2. authtoken 是否已設定：`ngrok config check`
3. 查看 ngrok 管理介面：http://localhost:4040
