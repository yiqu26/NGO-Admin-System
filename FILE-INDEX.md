# 📑 完整文件索引

> NGO 案管系統專案的所有文件快速查找指南

**最後更新**：2025-11-01

---

## 🎯 快速導航

| 文件類型 | 位置 | 說明 |
|---------|------|------|
| **主要文檔** | [README.md](README.md) | 專案主導航 |
| **快速啟動** | [QUICK-START.md](QUICK-START.md) | 3 分鐘快速啟動 |
| **文檔目錄** | [docs/README.md](docs/README.md) | 所有文檔索引 |
| **腳本說明** | [scripts/README.md](scripts/README.md) | 啟動腳本說明 |
| **配置說明** | [config/README.md](config/README.md) | 配置文件說明 |
| **資料庫說明** | [database/README.md](database/README.md) | 資料庫腳本說明 |

---

## 📂 完整文件結構

```
C:\Users\lanli\source\repos\
│
├── 📄 README.md                          # 專案主導航（從這裡開始）
├── 📄 QUICK-START.md                     # 快速啟動指南
├── 📄 FILE-INDEX.md                      # 本文件（完整文件索引）
│
├── 📂 docs\                              # 文檔資料夾
│   ├── 📄 README.md                      # 文檔目錄索引
│   ├── 📄 本地開發環境啟動指南.md
│   ├── 📄 工作總結與下次啟動指南.md
│   ├── 📄 展示系統使用說明.md
│   ├── 📄 測試帳號列表.md
│   ├── 📄 測試帳號與登入指南.md
│   ├── 📄 README-本地開發環境.md
│   └── 📄 部署指南-免費方案.md
│
├── 📂 scripts\                           # 啟動腳本資料夾
│   ├── 📄 README.md                      # 腳本說明文檔
│   ├── 📜 啟動展示系統.bat              # 完整展示模式
│   ├── 📜 重新啟動展示系統.bat          # 重啟展示系統
│   ├── 📜 啟動NGO展示系統.bat           # NGO 展示模式
│   ├── 📜 start-demo.bat                # Demo 模式
│   ├── 📜 start-ngrok-tunnel.bat        # ngrok tunnel
│   └── 📜 setup-github-repos.bat        # GitHub 設置
│
├── 📂 config\                            # 配置文件資料夾
│   ├── 📄 README.md                      # 配置說明文檔
│   ├── ⚙️ ngrok-config.yml              # ngrok 配置
│   ├── ⚙️ ngrok.yml                     # ngrok 設定
│   ├── ⚙️ create-test-account.json      # 測試帳號配置
│   ├── ⚙️ create-staff-account.json     # 員工帳號配置
│   └── ⚙️ create-supervisor-account.json # 督導帳號配置
│
├── 📂 database\                          # 資料庫腳本資料夾
│   ├── 📄 README.md                      # 資料庫說明文檔
│   ├── 💾 export-data.sql               # 資料匯出
│   ├── 💾 export-workers.sql            # 員工資料匯出
│   └── 💾 export-to-postgres.ps1        # PostgreSQL 匯出
│
├── 📂 NGO-Admin-System\                  # React 前端專案
├── 📂 NGO-Admin-System-WebAPI\           # WebAPI 後端專案
└── 📂 NGO-User-Case\                     # MVC 前台專案
```

---

## 📖 文檔文件詳細索引

### 主要導航文檔

| 文件 | 路徑 | 用途 | 閱讀時間 |
|------|------|------|---------|
| **專案主 README** | `README.md` | 專案總覽、快速導航 | 5 分鐘 |
| **快速啟動指南** | `QUICK-START.md` | 3 分鐘快速啟動 | 3 分鐘 |
| **完整文件索引** | `FILE-INDEX.md` | 本文件，所有文件清單 | 2 分鐘 |

---

### docs\ 文檔資料夾

| 文件 | 用途 | 適合對象 | 閱讀時間 |
|------|------|---------|---------|
| `README.md` | 文檔目錄索引 | 所有人 | 3 分鐘 |
| `測試帳號與登入指南.md` | 登入流程說明 | 新手 | 2 分鐘 |
| `測試帳號列表.md` | 測試帳號資訊 | 開發者 | 3 分鐘 |
| `本地開發環境啟動指南.md` | 詳細啟動步驟 | 開發者 | 10 分鐘 |
| `README-本地開發環境.md` | 系統總覽 | 開發者 | 5 分鐘 |
| `工作總結與下次啟動指南.md` | 快速參考 | 熟悉系統的人 | 5 分鐘 |
| `展示系統使用說明.md` | ngrok 展示 | 求職者 | 8 分鐘 |
| `部署指南-免費方案.md` | 雲端部署 | 部署人員 | 20 分鐘 |

---

## 🚀 腳本文件詳細索引

### scripts\ 腳本資料夾

| 腳本 | 功能 | 啟動內容 | 使用場景 |
|------|------|---------|---------|
| `README.md` | 腳本說明文檔 | - | 了解腳本用途 |
| `啟動展示系統.bat` | 完整展示模式 | 前端+後端+ngrok×2 | 面試展示 |
| `重新啟動展示系統.bat` | 重啟展示系統 | 同上 | 系統重啟 |
| `啟動NGO展示系統.bat` | NGO 展示模式 | 同上 | 展示系統 |
| `start-demo.bat` | Demo 模式 | 後端+ngrok API | 前端已部署 |
| `start-ngrok-tunnel.bat` | ngrok Tunnel | 資料庫 tunnel | 遠端連接 DB |
| `setup-github-repos.bat` | GitHub 設置 | Git 初始化 | 首次設置 |

**詳細說明**：[scripts/README.md](scripts/README.md)

---

## ⚙️ 配置文件詳細索引

### config\ 配置資料夾

| 文件 | 類型 | 用途 | 說明 |
|------|------|------|------|
| `README.md` | 文檔 | 配置說明 | 配置文件使用指南 |
| `ngrok-config.yml` | YAML | ngrok 配置 | ngrok 主要配置 |
| `ngrok.yml` | YAML | ngrok 設定 | ngrok 備用配置 |
| `create-test-account.json` | JSON | 測試帳號範本 | 一般測試帳號 |
| `create-staff-account.json` | JSON | 員工帳號範本 | 員工角色帳號 |
| `create-supervisor-account.json` | JSON | 督導帳號範本 | 督導角色帳號 |

**詳細說明**：[config/README.md](config/README.md)

---

## 💾 資料庫文件詳細索引

### database\ 資料庫資料夾

| 文件 | 類型 | 用途 | 說明 |
|------|------|------|------|
| `README.md` | 文檔 | 資料庫說明 | 資料庫腳本使用指南 |
| `export-data.sql` | SQL | 資料匯出 | 匯出完整資料庫 |
| `export-workers.sql` | SQL | 員工資料匯出 | 僅匯出社工帳號 |
| `export-to-postgres.ps1` | PowerShell | PostgreSQL 遷移 | SQL Server → PostgreSQL |

**詳細說明**：[database/README.md](database/README.md)

---

## 🔍 按用途查找文件

### 我需要...

#### 快速啟動系統
→ [QUICK-START.md](QUICK-START.md)
→ [工作總結與下次啟動指南](docs/工作總結與下次啟動指南.md)

#### 詳細啟動步驟
→ [本地開發環境啟動指南](docs/本地開發環境啟動指南.md)

#### 測試帳號
→ [測試帳號列表](docs/測試帳號列表.md)
→ [測試帳號與登入指南](docs/測試帳號與登入指南.md)

#### 展示系統
→ [展示系統使用說明](docs/展示系統使用說明.md)
→ `scripts\啟動展示系統.bat`

#### 雲端部署
→ [部署指南-免費方案](docs/部署指南-免費方案.md)

#### 設定 ngrok
→ [config/README.md](config/README.md)
→ `config\ngrok-config.yml`

#### 資料庫操作
→ [database/README.md](database/README.md)
→ `database\export-data.sql`

#### 了解系統架構
→ [README.md](README.md)
→ [README-本地開發環境](docs/README-本地開發環境.md)

---

## 📊 文件類型統計

| 類型 | 數量 | 位置 |
|------|------|------|
| **Markdown 文檔** | 12 | `*.md` |
| **啟動腳本** | 6 | `scripts\*.bat` |
| **配置文件** | 5 | `config\*.yml, *.json` |
| **資料庫腳本** | 3 | `database\*.sql, *.ps1` |
| **README 說明** | 5 | `*/README.md` |

**總計**：31 個文件

---

## 🎯 使用建議

### 新手使用路徑
1. 閱讀 [README.md](README.md) - 了解專案
2. 閱讀 [QUICK-START.md](QUICK-START.md) - 快速啟動
3. 閱讀 [測試帳號列表](docs/測試帳號列表.md) - 取得測試帳號
4. 執行啟動指令 - 開始使用

**預估時間**：10 分鐘

---

### 開發者使用路徑
1. 閱讀 [README.md](README.md)
2. 閱讀 [本地開發環境啟動指南](docs/本地開發環境啟動指南.md)
3. 配置環境
4. 開始開發

**預估時間**：20 分鐘

---

### 展示者使用路徑
1. 閱讀 [展示系統使用說明](docs/展示系統使用說明.md)
2. 設定 ngrok（參考 [config/README.md](config/README.md)）
3. 執行 `scripts\啟動展示系統.bat`
4. 取得公開網址

**預估時間**：15 分鐘

---

## 📝 文件更新記錄

| 日期 | 更新內容 |
|------|---------|
| 2025-11-01 | 完整重新整理所有文件，建立新的資料夾結構 |
| 2025-11-01 | 創建主 README.md 和 QUICK-START.md |
| 2025-11-01 | 為各資料夾創建 README.md 說明文檔 |
| 2025-11-01 | 創建本文件索引（FILE-INDEX.md） |

---

## 🔗 外部資源

### 專案 GitHub
- **前端**：https://github.com/yiqu26/NGO-Admin-System
- **後端**：https://github.com/yiqu26/NGO-Admin-System-WebAPI

### 技術文檔
- [React 官方文檔](https://react.dev)
- [ASP.NET Core 文檔](https://docs.microsoft.com/aspnet/core)
- [Vite 文檔](https://vitejs.dev)
- [Material-UI 文檔](https://mui.com)
- [ngrok 文檔](https://ngrok.com/docs)

### 部署平台
- [Vercel](https://vercel.com) - 前端部署
- [Render](https://render.com) - 後端部署
- [ngrok](https://ngrok.com) - Tunnel 服務

---

## 💡 快速參考

### 最常用的 5 個文件

1. **[QUICK-START.md](QUICK-START.md)** - 快速啟動
2. **[測試帳號列表](docs/測試帳號列表.md)** - 測試帳號
3. **[本地開發環境啟動指南](docs/本地開發環境啟動指南.md)** - 詳細步驟
4. **[scripts/README.md](scripts/README.md)** - 腳本說明
5. **[展示系統使用說明](docs/展示系統使用說明.md)** - 展示系統

---

### 最常用的 3 個腳本

1. **`scripts\啟動展示系統.bat`** - 完整展示模式
2. **`scripts\start-demo.bat`** - Demo 模式
3. **`scripts\start-ngrok-tunnel.bat`** - ngrok Tunnel

---

## 📞 需要幫助？

### 找不到需要的文件？
1. 使用本文件索引搜尋
2. 查看各資料夾的 README.md
3. 使用 VS Code 全域搜尋

### 文件內容有誤？
1. 直接編輯文件
2. 記錄問題和建議
3. 更新文檔

### 需要新增文件？
1. 確定文件類型
2. 放入對應資料夾
3. 更新相關的 README.md
4. 更新本索引文件

---

## ✨ 文件特色

### ✅ 完整性
- 涵蓋所有使用場景
- 從新手到進階
- 開發到部署

### ✅ 可讀性
- 清晰的結構
- 詳細的說明
- 豐富的範例

### ✅ 實用性
- 可複製的指令
- 實際操作步驟
- 問題排除指南

---

<div align="center">

**所有文件都已整理完成！** 🎉

從 [README.md](README.md) 或 [QUICK-START.md](QUICK-START.md) 開始使用吧！

</div>

---

**文件總數**：31 個文件
**最後更新**：2025-11-01
**版本**：v1.0
