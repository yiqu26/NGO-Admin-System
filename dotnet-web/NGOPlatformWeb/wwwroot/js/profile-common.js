/**
 * NGO Platform - Profile Common JavaScript
 * 個人資料頁面共用 JavaScript 函數
 */

/**
 * 統一的消息提示函數
 * @param {string} message - 要顯示的消息
 * @param {string} type - 消息類型 ('success' 或 'error')
 */
function showProfileMessage(message, type) {
    // 創建提示訊息
    const messageDiv = document.createElement('div');
    messageDiv.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show position-fixed`;
    messageDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    messageDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(messageDiv);

    // 3秒後自動移除
    setTimeout(() => {
        if (messageDiv.parentNode) {
            messageDiv.parentNode.removeChild(messageDiv);
        }
    }, 3000);
}

/**
 * 統一的頭像上傳函數
 * @param {HTMLInputElement} input - 文件輸入元素
 * @param {string} uploadUrl - 上傳 API 網址
 * @param {string} displayElementId - 顯示頭像的元素 ID
 * @param {string} overlaySelector - 覆蓋層選擇器
 */
async function uploadProfileImage(input, uploadUrl, displayElementId, overlaySelector) {
    const file = input.files[0];
    if (!file) return;

    // 檢查檔案類型
    if (!file.type.match(/image\/(jpeg|jpg|png)/)) {
        showProfileMessage('只允許上傳 JPG 或 PNG 格式的圖片', 'error');
        return;
    }

    // 檢查檔案大小 (2MB)
    if (file.size > 2 * 1024 * 1024) {
        showProfileMessage('檔案大小不能超過 2MB', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('profileImage', file);

    try {
        // 顯示上傳中狀態
        const overlay = document.querySelector(overlaySelector);
        const originalContent = overlay.innerHTML;
        overlay.classList.add('uploading');
        overlay.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>上傳中...</span>';

        const response = await fetch(uploadUrl, {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            // 更新頭像顯示
            document.getElementById(displayElementId).src = result.imageUrl + '?t=' + new Date().getTime();
            
            // 顯示成功訊息
            showProfileMessage('頭像更新成功！', 'success');
        } else {
            showProfileMessage(result.message || '上傳失敗', 'error');
        }

        // 恢復原始狀態
        overlay.classList.remove('uploading');
        overlay.innerHTML = originalContent;

    } catch (error) {
        console.error('上傳錯誤:', error);
        showProfileMessage('上傳失敗，請稍後再試', 'error');
        
        // 恢復原始狀態
        const overlay = document.querySelector(overlaySelector);
        overlay.classList.remove('uploading');
        overlay.innerHTML = '<i class="fas fa-camera"></i><span>更換頭像</span>';
    }

    // 清空 input
    input.value = '';
}

/**
 * 預覽圖片函數（用於編輯頁面）
 * @param {HTMLInputElement} input - 文件輸入元素
 * @param {string} previewElementId - 預覽元素 ID
 */
function previewProfileImage(input, previewElementId) {
    const file = input.files[0];
    if (file) {
        // 檢查檔案類型
        if (!file.type.match(/image\/(jpeg|jpg|png)/)) {
            showProfileMessage('只允許上傳 JPG 或 PNG 格式的圖片', 'error');
            input.value = '';
            return;
        }

        // 檢查檔案大小 (2MB)
        if (file.size > 2 * 1024 * 1024) {
            showProfileMessage('檔案大小不能超過 2MB', 'error');
            input.value = '';
            return;
        }

        // 預覽圖片
        const reader = new FileReader();
        reader.onload = function(e) {
            document.getElementById(previewElementId).src = e.target.result;
        };
        reader.readAsDataURL(file);
    }
}