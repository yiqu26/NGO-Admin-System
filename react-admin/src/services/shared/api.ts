import axios, { InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import { config } from '../../config/env';

// API 基礎配置
const API_BASE_URL = config.apiBaseUrl;

/**
 * 創建 Axios 實例
 * 配置統一的 API 請求設定，包括基礎 URL、超時時間和預設標頭
 */
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: config.requestTimeout,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * 請求攔截器 (Request Interceptor)
 * 
 * 功能：
 * 1. 自動添加身份驗證令牌到請求標頭
 * 2. 統一處理請求前的數據處理
 */
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: unknown) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

/**
 * API 服務物件 (API Service)
 * 
 * 提供統一的 HTTP 請求方法，封裝 Axios 操作
 */
export const api = {
  get: <T>(url: string, params?: Record<string, unknown>): Promise<T> => {
    if (params) {
      const queryString = Object.keys(params)
        .filter(key => params[key] !== undefined && params[key] !== null && params[key] !== '')
        .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(String(params[key]))}`)
        .join('&');
      if (queryString) url = `${url}?${queryString}`;
    }
    return apiClient.get<T>(url).then(res => res.data);
  },

  post: <T>(url: string, data?: unknown, config?: object): Promise<T> =>
    apiClient.post<T>(url, data, config).then(res => res.data),

  put: <T>(url: string, data?: unknown): Promise<T> =>
    apiClient.put<T>(url, data).then(res => res.data),

  delete: <T>(url: string): Promise<T> =>
    apiClient.delete<T>(url).then(res => res.data),

  patch: <T>(url: string, data?: unknown): Promise<T> =>
    apiClient.patch<T>(url, data).then(res => res.data),
};

export default apiClient; 