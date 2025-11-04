# ⚙️ 配置文件說明

本資料夾包含系統配置文件，用於設定 ngrok 和測試帳號。

---

## 📋 文件列表

### ngrok 配置

#### 1. `ngrok-config.yml`
**用途**：ngrok 主要配置文件

**內容**：
```yaml
version: "2"
authtoken: YOUR_AUTHTOKEN_HERE

tunnels:
  sql-server:
    proto: tcp
    addr: 1433
```

**使用方式**：
```batch
# 使用配置文件啟動 ngrok
ngrok start --all --config ngrok-config.yml
```

**設定步驟**：
1. 註冊 ngrok 帳號：https://ngrok.com
2. 取得 authtoken：https://dashboard.ngrok.com/get-started/your-authtoken
3. 替換 `YOUR_AUTHTOKEN_HERE` 為實際的 authtoken
4. 儲存文件

**支援的 tunnel 類型**：
- `http`: HTTP/HTTPS 流量
- `tcp`: TCP 流量（資料庫連線）
- `tls`: TLS 流量

---

#### 2. `ngrok.yml`
**用途**：ngrok 備用配置文件

**與 ngrok-config.yml 的差異**：
- 內容相似，用途相同
- 可作為備份或測試用

**使用方式**：
```batch
ngrok start --all --config ngrok.yml
```

---

### 測試帳號配置

#### 3. `create-test-account.json`
**用途**：一般測試帳號配置範本

**內容範例**：
```json
{
  "email": "test@ngo.org",
  "password": "Test123!",
  "name": "測試用戶",
  "role": "staff"
}
```

**使用方式**：
```batch
# 使用 curl 或 Postman 發送 POST 請求
curl -X POST "http://localhost:5264/api/Worker" \
  -H "Content-Type: application/json" \
  -d @create-test-account.json
```

**欄位說明**：
- `email`: 登入用 Email（必填，需唯一）
- `password`: 密碼（必填，需符合密碼規則）
- `name`: 顯示名稱（必填）
- `role`: 角色（必填：`admin`, `supervisor`, `staff`）

---

#### 4. `create-staff-account.json`
**用途**：員工帳號配置範本

**預設角色**：`staff`（員工）

**權限**：
- ✅ 查看個案
- ✅ 查看活動
- ✅ 查看物資
- ❌ 刪除資料
- ❌ 管理帳號

**使用場景**：
- 建立一般社工帳號
- 測試員工權限

---

#### 5. `create-supervisor-account.json`
**用途**：督導帳號配置範本

**預設角色**：`supervisor`（督導）

**權限**：
- ✅ 查看個案
- ✅ 新增/編輯個案
- ✅ 審核活動報名
- ✅ 管理物資
- ✅ 查看統計報表
- ❌ 管理系統帳號

**使用場景**：
- 建立督導帳號
- 測試督導權限

---

## 🔧 配置修改指南

### 修改 ngrok authtoken

**步驟**：
1. 編輯 `ngrok-config.yml`
2. 找到 `authtoken: YOUR_AUTHTOKEN_HERE`
3. 替換為實際的 authtoken
4. 儲存

**範例**：
```yaml
authtoken: 2abc123xyz456def789ghi
```

---

### 新增 ngrok tunnel

**步驟**：
1. 編輯 `ngrok-config.yml`
2. 在 `tunnels:` 下新增 tunnel
3. 儲存並重新啟動 ngrok

**範例**：
```yaml
tunnels:
  sql-server:
    proto: tcp
    addr: 1433

  frontend:
    proto: http
    addr: 5173

  backend:
    proto: http
    addr: 5264
```

---

### 建立自訂測試帳號

**步驟**：
1. 複製 `create-test-account.json`
2. 修改內容：
   ```json
   {
     "email": "mytest@ngo.org",
     "password": "MyPassword123!",
     "name": "我的測試帳號",
     "role": "staff"
   }
   ```
3. 儲存為新文件（例如：`create-my-account.json`）
4. 使用 API 建立帳號

---

## 📝 密碼規則

建立帳號時，密碼必須符合以下規則：

- ✅ 至少 8 個字元
- ✅ 包含大寫字母（A-Z）
- ✅ 包含小寫字母（a-z）
- ✅ 包含數字（0-9）
- ✅ 包含特殊符號（!@#$%^&*）

**有效範例**：
- `Admin123!`
- `Super123!`
- `Staff123!`
- `MyPassword1!`

**無效範例**：
- `admin123` ❌ (缺少大寫和特殊符號)
- `ADMIN123!` ❌ (缺少小寫)
- `Admin!` ❌ (太短，缺少數字)

---

## 🎯 角色權限對照表

| 功能 | admin | supervisor | staff |
|------|-------|------------|-------|
| **個案管理** |
| 查看個案 | ✅ | ✅ | ✅ |
| 新增個案 | ✅ | ✅ | ✅ |
| 編輯個案 | ✅ | ✅ | ✅ |
| 刪除個案 | ✅ | ✅ | ❌ |
| **活動管理** |
| 查看活動 | ✅ | ✅ | ✅ |
| 建立活動 | ✅ | ✅ | ✅ |
| 審核報名 | ✅ | ✅ | ❌ |
| 刪除活動 | ✅ | ✅ | ❌ |
| **物資管理** |
| 查看物資 | ✅ | ✅ | ✅ |
| 管理庫存 | ✅ | ✅ | ✅ |
| 物資分配 | ✅ | ✅ | ❌ |
| **系統管理** |
| 帳號管理 | ✅ | ❌ | ❌ |
| 系統設定 | ✅ | ❌ | ❌ |
| 查看日誌 | ✅ | ✅ | ❌ |

---

## 🚨 常見問題

### Q1: ngrok 顯示 "Invalid authtoken"
**原因**：authtoken 設定錯誤

**解決方式**：
1. 前往 https://dashboard.ngrok.com/get-started/your-authtoken
2. 複製正確的 authtoken
3. 更新 `ngrok-config.yml`
4. 重新啟動 ngrok

---

### Q2: 建立帳號失敗
**可能原因**：
1. Email 已被使用
2. 密碼不符合規則
3. JSON 格式錯誤

**解決方式**：
- 檢查 Email 是否重複
- 確認密碼符合規則
- 使用 JSON 驗證工具檢查格式

---

### Q3: ngrok tunnel 無法啟動
**可能原因**：
1. Port 被占用
2. 配置文件格式錯誤
3. ngrok 服務問題

**解決方式**：
```batch
# 測試單一 tunnel
ngrok http 5264

# 檢查 YAML 格式
# 使用線上 YAML validator
```

---

## 📚 相關文檔

- [ngrok 官方文檔](https://ngrok.com/docs)
- [展示系統使用說明](../docs/展示系統使用說明.md)
- [部署指南](../docs/部署指南-免費方案.md)
- [測試帳號列表](../docs/測試帳號列表.md)

---

## 💡 使用提示

### 提示 1: ngrok 固定網域
ngrok 付費版提供固定網域，避免每次重啟更換網址：
- **費用**：$8/月
- **優點**：網址固定，不需要每次更新配置

### 提示 2: 環境變數
可以將 authtoken 設定為環境變數，避免寫在配置文件中：
```batch
set NGROK_AUTHTOKEN=your_token_here
ngrok http 5264
```

### 提示 3: 批次建立帳號
可以使用 PowerShell 或 Python 腳本批次建立多個測試帳號：
```powershell
$accounts = @("test1", "test2", "test3")
foreach ($account in $accounts) {
    Invoke-RestMethod -Method Post -Uri "http://localhost:5264/api/Worker" `
        -ContentType "application/json" `
        -Body (@{
            email = "$account@ngo.org"
            password = "Test123!"
            name = "測試用戶 $account"
            role = "staff"
        } | ConvertTo-Json)
}
```

---

**最後更新**：2025-11-01
**配置版本**：v1.0
