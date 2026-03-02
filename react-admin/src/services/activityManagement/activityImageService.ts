import { api } from '../shared/api';

export interface ImageGenerationRequest {
  prompt: string;
}

export interface ImageGenerationResponse {
  success: boolean;
  imageData?: string;
  message: string;
}

export interface TestConnectionResponse {
  success: boolean;
  message: string;
}

class ImageGenerationService {
  /**
   * 使用 AI 生成圖片
   * @param prompt 圖片描述
   * @returns 生成的圖片數據
   */
  async generateImage(prompt: string): Promise<ImageGenerationResponse> {
    try {
      console.log('🚀 開始調用 AI 圖片生成 API，描述:', prompt);
      
      // 使用更長的超時時間（120秒）來處理 AI 圖片生成
      const response = await api.post('/ActivityImageGenerator/generate', {
        prompt: prompt
      }, {
        timeout: 120000 // 120秒超時
      });

      console.log('✅ API 調用成功，響應:', response);
      return response;
    } catch (error: any) {
      console.error('❌ 圖片生成 API 調用失敗:', error);
      console.error('錯誤詳情:', {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        config: error.config
      });
      
      // 檢查是否是超時錯誤
      if (error.code === 'ECONNABORTED' || error.message.includes('timeout')) {
        return {
          success: false,
          message: '圖片生成超時，請稍後再試（AI 圖片生成通常需要 10-30 秒，請確保網絡連接穩定）'
        };
      }
      
      if (error.response?.data) {
        console.log('返回錯誤響應數據:', error.response.data);
        return error.response.data;
      }
      
      return {
        success: false,
        message: '圖片生成失敗，請稍後再試'
      };
    }
  }

  /**
   * 測試 AI 圖片生成服務連接
   * @returns 測試結果
   */
  async testConnection(): Promise<TestConnectionResponse> {
    try {
      const response = await api.post('/ActivityImageGenerator/test-connection');
      return response.data;
    } catch (error: any) {
      console.error('連接測試失敗:', error);
      
      if (error.response?.data) {
        return error.response.data;
      }
      
      return {
        success: false,
        message: '連接測試失敗，請檢查服務配置'
      };
    }
  }

  /**
   * 將 Base64 圖片數據轉換為 File 對象
   * @param base64Data Base64 圖片數據
   * @param fileName 文件名
   * @returns File 對象
   */
  base64ToFile(base64Data: string, fileName: string = 'generated-image.png'): File {
    // 移除 data:image/png;base64, 前綴
    const base64WithoutPrefix = base64Data.replace(/^data:image\/[a-z]+;base64,/, '');
    
    // 將 Base64 轉換為 Blob
    const byteCharacters = atob(base64WithoutPrefix);
    const byteNumbers = new Array(byteCharacters.length);
    
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'image/png' });
    
    // 創建 File 對象
    return new File([blob], fileName, { type: 'image/png' });
  }

  /**
   * 驗證圖片描述
   * @param prompt 圖片描述
   * @returns 驗證結果
   */
  validatePrompt(prompt: string): { isValid: boolean; message: string } {
    if (!prompt || prompt.trim().length === 0) {
      return {
        isValid: false,
        message: '請輸入圖片描述'
      };
    }

    if (prompt.trim().length < 5) {
      return {
        isValid: false,
        message: '圖片描述至少需要 5 個字符'
      };
    }

    if (prompt.trim().length > 1000) {
      return {
        isValid: false,
        message: '圖片描述不能超過 1000 個字符'
      };
    }

    return {
      isValid: true,
      message: ''
    };
  }
}

const imageGenerationService = new ImageGenerationService();
export default imageGenerationService; 