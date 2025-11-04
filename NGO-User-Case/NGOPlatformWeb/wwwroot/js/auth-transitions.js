// 認證頁面切換動畫效果
// 立即執行的預設定，避免FOUC (Flash of Unstyled Content)
(function() {
    // 如果是頁面跳轉進來的，立即設置CSS變數
    if (sessionStorage.getItem('skipLoadingAnimation') === 'true') {
        document.documentElement.style.setProperty('--initial-content-opacity', '1');
        document.documentElement.style.setProperty('--initial-loading-display', 'none');
    }
})();

document.addEventListener('DOMContentLoaded', function() {
    
    // 優化的頁面切換動畫 - 零閃現版本
    const animatePageTransition = (targetUrl, delay = 30) => {
        // 清除所有動畫相關標記，設置跳轉標記
        sessionStorage.removeItem('showPlaneAnimation');
        sessionStorage.setItem('skipLoadingAnimation', 'true');
        
        // 預渲染覆蓋層到body最前面
        const overlay = document.createElement('div');
        overlay.id = 'page-transition-overlay';
        overlay.style.cssText = `
            position: fixed !important;
            top: 0 !important;
            left: 0 !important;
            width: 100vw !important;
            height: 100vh !important;
            background: linear-gradient(135deg, #374151 0%, #4b5563 50%, #374151 100%) !important;
            z-index: 999999 !important;
            opacity: 1 !important;
            pointer-events: none !important;
            will-change: opacity !important;
        `;
        
        // 插入到body第一個位置，立即覆蓋
        document.body.insertBefore(overlay, document.body.firstChild);
        
        // 立即跳轉，不等待
        setTimeout(() => {
            window.location.href = targetUrl;
        }, delay);
    };

    // 為所有認證相關鏈接添加動畫
    const authLinks = document.querySelectorAll('a[href*="Login"], a[href*="Register"], a[href*="ForgotPassword"], a[href*="ResetPassword"]');
    
    authLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            // 只對認證頁面間的跳轉添加動畫
            const currentPath = window.location.pathname.toLowerCase();
            const targetPath = this.getAttribute('href').toLowerCase();
            
            // 檢查是否為認證頁面間的跳轉
            const authPages = ['login', 'register', 'forgotpassword', 'resetpassword'];
            const isCurrentAuthPage = authPages.some(page => currentPath.includes(page));
            const isTargetAuthPage = authPages.some(page => targetPath.includes(page));
            
            if (isCurrentAuthPage && isTargetAuthPage) {
                e.preventDefault();
                animatePageTransition(this.href);
            }
        });
    });

    // 表單提交成功後的跳轉動畫
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            // 為表單提交添加視覺反饋
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    submitBtn.style.transform = 'scale(1)';
                }, 150);
            }
        });
    });

    // 鏈接hover效果增強
    authLinks.forEach(link => {
        link.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-1px)';
            this.style.transition = 'all 0.2s ease';
        });
        
        link.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // 為新載入的頁面添加進入效果 - 零閃現版本
    const mainContent = document.getElementById('mainContent');
    if (mainContent && sessionStorage.getItem('skipLoadingAnimation') === 'true') {
        // 立即創建滿屏覆蓋層，確保無閃現
        const overlay = document.createElement('div');
        overlay.id = 'page-enter-overlay';
        overlay.style.cssText = `
            position: fixed !important;
            top: 0 !important;
            left: 0 !important;
            width: 100vw !important;
            height: 100vh !important;
            background: linear-gradient(135deg, #374151 0%, #4b5563 50%, #374151 100%) !important;
            z-index: 999999 !important;
            opacity: 1 !important;
            pointer-events: none !important;
            transition: opacity 0.15s ease-out !important;
            will-change: opacity !important;
        `;
        
        // 立即插入到最前面
        document.body.insertBefore(overlay, document.body.firstChild);
        
        // 使用多個渲染幀確保頁面完全渲染
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    // 開始淡出
                    overlay.style.opacity = '0';
                    
                    // 淡出完成後移除
                    setTimeout(() => {
                        if (overlay && overlay.parentNode) {
                            overlay.remove();
                        }
                    }, 150);
                });
            });
        });
        
        // 清除跳轉標記
        sessionStorage.removeItem('skipLoadingAnimation');
    }
});