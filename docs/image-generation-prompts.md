# NGO 平台 — 需要產生的圖片清單

產完後將檔案依照「存檔名稱」命名，放入 `dotnet-web/NGOPlatformWeb/wwwroot/images/`

**Gemini 操作方式**：每次產圖選擇 **「橫向 / Landscape」** 比例即可，不需要指定像素數字。

---

## 優先順序 1：組織介紹頁

### 圖片 1
- **存檔名稱**：`organization-goal-hero.jpg`
- **Gemini 比例**：橫向 4:3
- **用途**：組織介紹頁目標大卡的背景圖片（會疊白色文字在上面）
- **Prompt**：
  A close-up photograph of a volunteer adult gently helping a child with schoolwork at a simple wooden table. Warm soft natural light from a window. Slightly blurred background (bokeh). Emotional and authentic atmosphere. Muted warm tones — amber, soft brown. The image should be somewhat dark overall so white text can be overlaid. Photorealistic, documentary style, no staged look.

### 圖片 2
- **存檔名稱**：`organization.jpg`
- **Gemini 比例**：橫向 16:9
- **用途**：組織介紹頁頂部 Hero 橫幅背景
- **Prompt**：
  Wide-angle photograph of a small community gathering in rural Taiwan. Families and volunteers outdoors in warm afternoon sunlight. Children in the background. A sense of hope and community connection. Natural colors — greens, warm earth tones. Slightly cinematic. The bottom third is darker to allow text overlay. Photorealistic, not staged.

---

## 優先順序 2：首頁輪播（3 張）

### 圖片 3
- **存檔名稱**：`homepageC1.png`
- **Gemini 比例**：橫向 16:9
- **用途**：首頁輪播第一張，標題「建立更美好的社會」
- **Prompt**：
  Wide photograph of volunteers and children in a bright outdoor community space. Warm golden hour light. People smiling and interacting naturally, not posed. Lush greenery in background. The left side of the composition has slightly less detail, leaving visual room for text. Photorealistic, warm and hopeful mood. Slightly darkened overall.

### 圖片 4
- **存檔名稱**：`homepageC2.png`
- **Gemini 比例**：橫向 16:9
- **用途**：首頁輪播第二張，標題「志工招募進行中」
- **Prompt**：
  A group of young volunteers in casual clothes, laughing and working together outdoors. Some carrying supply boxes, some talking in small groups. Bright daylight, energetic and positive atmosphere. Candid style, not posed. Slightly darkened for text readability. Photorealistic.

### 圖片 5
- **存檔名稱**：`homepageC3.png`
- **Gemini 比例**：橫向 16:9
- **用途**：首頁輪播第三張，標題「社區服務活動」
- **Prompt**：
  A lively community service event. People of various ages participating — distributing supplies, helping children, folding donations. Warm vibrant colors. Natural light. A candid, documentary feel. Slightly elevated angle. Darkened bottom for text overlay. Photorealistic.

---

## 優先順序 3：其他頁面背景

### 圖片 6
- **存檔名稱**：`contact.jpg`
- **Gemini 比例**：橫向 16:9
- **用途**：聯絡我們頁頂部 Hero 背景
- **Prompt**：
  Interior of a small warm NGO office. A social worker at a desk speaking with a family. Soft indoor lighting. Clean, organized space with a few plants. Warm beige and wood tones. Slightly blurred to serve as background. Photorealistic, authentic, not corporate-looking.

### 圖片 7
- **存檔名稱**：`activityback.jpg`
- **Gemini 比例**：橫向 16:9
- **用途**：首頁使命陳述區塊背景（會被深色遮罩覆蓋）
- **Prompt**：
  Elevated wide view of an outdoor community activity in a park. Many people gathered, colorful clothing, warm afternoon light. Joyful atmosphere. Slightly desaturated and dark overall — this image will have a dark overlay applied on top. Photorealistic.

### 圖片 8
- **存檔名稱**：`activityback2.jpg`
- **Gemini 比例**：橫向 21:9（超寬幅，選這個才不會被裁掉重點）
- **用途**：活動總覽頁頂部 Hero 背景（網頁顯示很扁的橫條）
- **Prompt**：
  Children and adults in a structured outdoor educational activity — sitting in a circle doing crafts or listening to a speaker. Bright natural light. Open and airy composition. Green grass and trees visible. Warm and inviting mood. Photorealistic.

### 圖片 9
- **存檔名稱**：`Caseshoppingback.jpg`
- **Gemini 比例**：橫向 21:9
- **用途**：物資申請頁頂部 Hero 背景（同樣很扁的橫條）
- **Prompt**：
  Neatly organized donated supplies on shelves or tables — rice bags, folded clothing, household items arranged cleanly. Warm light. A volunteer's hands visible organizing items. Clean, orderly atmosphere. Slightly dark. Photorealistic.

### 圖片 10
- **存檔名稱**：`LoginImage.png`
- **Gemini 比例**：直向 9:16
- **用途**：登入頁右側背景（直向長圖）
- **Prompt**：
  Tall portrait-format photograph. Soft blurred background of a community garden or neighborhood during golden hour. Warm amber and teal tones. Abstract enough to not distract — gentle bokeh. No faces. Overall dark (about 60% brightness) to work under a dark UI overlay. Photorealistic, cinematic feel.

---

## 注意事項

- 全部選**寫實照片風格（photorealistic / photo）**，不要插畫或卡通
- 如果產出太亮：在 prompt 最後加 `slightly underexposed, darker overall`
- 如果色調太冷：加 `warm color temperature, amber tones`
- 產完存檔時副檔名照上面列的（.jpg 或 .png），大小寫要一致
