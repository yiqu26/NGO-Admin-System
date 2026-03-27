using NGOPlatformWeb.Models.Entity;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Web;
using Microsoft.EntityFrameworkCore;

namespace NGOPlatformWeb.Services
{
    public class EcpayService
    {
        private readonly NGODbContext _context;
        private readonly IConfiguration _configuration;

        private readonly string _merchantId;
        private readonly string _hashKey;
        private readonly string _hashIv;
        private readonly string _paymentUrl;

        public EcpayService(NGODbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            // 從 appsettings.json 讀取，方便日後切換測試/正式環境不需修改程式碼
            _merchantId = configuration["ECPay:MerchantId"] ?? throw new InvalidOperationException("ECPay:MerchantId 未設定");
            _hashKey = configuration["ECPay:HashKey"] ?? throw new InvalidOperationException("ECPay:HashKey 未設定");
            _hashIv = configuration["ECPay:HashIv"] ?? throw new InvalidOperationException("ECPay:HashIv 未設定");
            _paymentUrl = configuration["ECPay:PaymentUrl"] ?? throw new InvalidOperationException("ECPay:PaymentUrl 未設定");
        }

        public string CreatePayment(UserOrder order, string returnUrl, string clientBackUrl)
        {
            try
            {
                // 檢查是否使用 Ngrok 進行本地測試
                var useNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                var ngrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");
                
                if (useNgrok && !string.IsNullOrEmpty(ngrokUrl))
                {
                    // 替換所有可能的 localhost 變體
                    returnUrl = returnUrl.Replace("https://localhost:7210", ngrokUrl)
                                        .Replace("http://localhost:7210", ngrokUrl)
                                        .Replace("https://localhost:5066", ngrokUrl)
                                        .Replace("http://localhost:5066", ngrokUrl);
                    clientBackUrl = clientBackUrl.Replace("https://localhost:7210", ngrokUrl)
                                                 .Replace("http://localhost:7210", ngrokUrl)
                                                 .Replace("https://localhost:5066", ngrokUrl)
                                                 .Replace("http://localhost:5066", ngrokUrl);
                }
                // 查找或建立 ECPay 交易記錄
                var existingTransaction = _context.EcpayTransactions
                    .FirstOrDefault(e => e.UserOrderId == order.UserOrderId);
                
                EcpayTransaction ecpayTransaction;
                if (existingTransaction != null)
                {
                    // 已付款訂單不允許重新觸發付款，防止重複扣款與庫存雙重更新
                    if (existingTransaction.Status == "Success")
                        throw new InvalidOperationException("此訂單已付款成功，不允許重複付款");

                    // 重新付款：更新現有記錄
                    ecpayTransaction = existingTransaction;
                    ecpayTransaction.Status = "Pending";
                    ecpayTransaction.CreatedDateTime = DateTime.Now;
                    ecpayTransaction.ResponseData = null; // 清空之前的回應資料
                }
                else
                {
                    // 首次付款：建立新記錄
                    ecpayTransaction = new EcpayTransaction
                    {
                        UserOrderId = order.UserOrderId,
                        Status = "Pending",
                        CreatedDateTime = DateTime.Now
                    };
                    _context.EcpayTransactions.Add(ecpayTransaction);
                }
                
                _context.SaveChanges();

            // 建立付款參數 - 使用訂單號作為 MerchantTradeNo
            var parameters = new Dictionary<string, string>
            {
                ["MerchantID"] = _merchantId,
                ["MerchantTradeNo"] = order.OrderNumber, // 使用資料庫中的訂單號
                ["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                ["PaymentType"] = "aio",
                ["TotalAmount"] = ((int)order.TotalPrice).ToString(),
                ["TradeDesc"] = "NGO物資捐贈",
                ["ItemName"] = "物資捐贈",
                ["ReturnURL"] = returnUrl,
                ["ClientBackURL"] = clientBackUrl, // 添加付款完成後跳轉網址
                ["ChoosePayment"] = "ALL",
                ["EncryptType"] = "1"
            };

            // 移除空值參數（CreditInstallment 為空時不應包含）
            var finalParams = parameters.Where(p => !string.IsNullOrEmpty(p.Value)).ToDictionary(p => p.Key, p => p.Value);

            
            // 生成檢查碼
            var checkMacValue = GenerateCheckMacValue(finalParams, _hashKey, _hashIv);
            finalParams["CheckMacValue"] = checkMacValue;

            // 產生 HTML 表單
            var formHtml = GeneratePaymentForm(finalParams);
            

                // 更新 ECPay 交易記錄
                ecpayTransaction.EcpayTradeNo = order.OrderNumber;
                ecpayTransaction.Status = "Processing";
                _context.SaveChanges();

                return formHtml;
            }
            catch (Exception ex)
            {
                throw new Exception($"建立付款時發生錯誤: {ex.Message}", ex);
            }
        }

        public bool ProcessCallback(Dictionary<string, string> parameters)
        {
            try
            {
                var merchantTradeNo = parameters.GetValueOrDefault("MerchantTradeNo");
                var rtnCode = parameters.GetValueOrDefault("RtnCode");
                var tradeNo = parameters.GetValueOrDefault("TradeNo");
                var receivedCheckMacValue = parameters.GetValueOrDefault("CheckMacValue");

                if (string.IsNullOrEmpty(merchantTradeNo) || string.IsNullOrEmpty(receivedCheckMacValue))
                    return false;

                // **安全驗證：使用專門的回調驗證方法**
                var calculatedCheckMacValue = GenerateCallbackCheckMacValue(parameters, _hashKey, _hashIv);
                if (calculatedCheckMacValue != receivedCheckMacValue)
                {
                    // CheckMacValue 不匹配，這可能是偽造的回調
                    // 在生產環境中應該拒絕此回調
                    return false;
                }

                // 查找訂單和交易記錄
                var order = _context.UserOrders.FirstOrDefault(o => o.OrderNumber == merchantTradeNo);
                var ecpayTransaction = _context.EcpayTransactions.FirstOrDefault(e => e.EcpayTradeNo == merchantTradeNo);

                if (order == null || ecpayTransaction == null)
                    return false;

                // 更新交易記錄
                ecpayTransaction.ResponseData = JsonSerializer.Serialize(parameters);
                
                if (rtnCode == "1") // 付款成功且 CheckMacValue 驗證通過
                {
                    // 冪等保護：已處理過的成功回調直接回傳，防止 ECPay 重送時重複更新庫存
                    if (ecpayTransaction.Status == "Success")
                        return true;

                    ecpayTransaction.Status = "Success";
                    order.PaymentStatus = "已付款";

                    // 付款成功後，更新庫存和緊急需求
                    UpdateInventoryAndEmergencyNeeds(order);
                }
                else // 付款失敗
                {
                    ecpayTransaction.Status = "Failed";
                    order.PaymentStatus = "付款失敗";
                }

                _context.SaveChanges();
                return rtnCode == "1";
            }
            catch (Exception ex)
            {
                // 記錄錯誤以供監控和調試
                Console.WriteLine($"ECPay 回調處理錯誤: {ex.Message}");
                return false;
            }
        }

        // 付款創建時的 CheckMacValue 計算（保持原有邏輯不變）
        private string GenerateCheckMacValue(Dictionary<string, string> parameters, string hashKey, string hashIv)
        {
            try
            {
                // 1. 複製參數並轉換為字串
                var stringParams = new Dictionary<string, string>();
                foreach (var param in parameters)
                {
                    if (param.Key != "CheckMacValue" && !string.IsNullOrEmpty(param.Value))
                    {
                        stringParams[param.Key] = param.Value.ToString();
                    }
                }
                
                // 2. 排序參數 (嚴格按照字典順序)
                var sortedParams = stringParams.OrderBy(x => x.Key).ToList();
                
                // 3. 建立查詢字串
                var queryString = string.Join("&", sortedParams.Select(p => $"{p.Key}={p.Value}"));
                
                // 4. 組合最終字串
                var checkString = $"HashKey={hashKey}&{queryString}&HashIV={hashIv}";
                
                // 5. URL 編碼 (統一使用 HttpUtility.UrlEncode)
                var encoded = System.Web.HttpUtility.UrlEncode(checkString, Encoding.UTF8);
                
                // 6. 轉小寫
                encoded = encoded.ToLower();
                
                // 7. 字符替換 (按照 ECPay 規範)
                encoded = encoded.Replace("%21", "!")
                               .Replace("%2a", "*")
                               .Replace("%28", "(") 
                               .Replace("%29", ")")
                               .Replace("%2d", "-")
                               .Replace("%5f", "_")
                               .Replace("%2e", ".");
                
                // 8. SHA256 加密
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encoded));
                    var result = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CheckMacValue 計算錯誤: {ex.Message}", ex);
            }
        }

        // 專門處理 ECPay 回調的 CheckMacValue 驗證
        private string GenerateCallbackCheckMacValue(Dictionary<string, string> parameters, string hashKey, string hashIv)
        {
            try
            {
                // 1. 移除 CheckMacValue 參數，但保留所有其他參數（包括空值）
                var filteredParams = parameters
                    .Where(kv => kv.Key != "CheckMacValue")
                    .OrderBy(kv => kv.Key)
                    .ToList();
                
                // 2. 組合查詢字串（保留空值欄位）
                var queryString = string.Join("&", filteredParams.Select(kv => $"{kv.Key}={kv.Value}"));
                
                // 3. 組合最終字串
                var checkString = $"HashKey={hashKey}&{queryString}&HashIV={hashIv}";
                
                // 4. URL 編碼
                var encoded = System.Web.HttpUtility.UrlEncode(checkString, Encoding.UTF8).ToLower();
                
                // 5. 字符替換（按照 ECPay 規範）
                encoded = encoded
                    .Replace("%21", "!")
                    .Replace("%28", "(")
                    .Replace("%29", ")")
                    .Replace("%2a", "*")
                    .Replace("%2d", "-")
                    .Replace("%2e", ".")
                    .Replace("%5f", "_");
                
                // 6. SHA256 加密
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encoded));
                    var result = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"回調 CheckMacValue 計算錯誤: {ex.Message}", ex);
            }
        }


        private string GeneratePaymentForm(Dictionary<string, string> parameters)
        {
            var formBuilder = new StringBuilder();
            formBuilder.AppendLine("<form id='ecpayForm' method='post' action='" + _paymentUrl + "'>");
            
            foreach (var param in parameters)
            {
                // HtmlEncode 防止 value 中含有單引號或特殊字元破壞 HTML 屬性結構（XSS）
                var encodedValue = System.Net.WebUtility.HtmlEncode(param.Value);
                formBuilder.AppendLine($"<input type='hidden' name='{param.Key}' value='{encodedValue}' />");
            }
            
            formBuilder.AppendLine("<input type='submit' value='前往付款' style='padding: 10px 20px; background: #007bff; color: white; border: none; border-radius: 5px; cursor: pointer;' />");
            formBuilder.AppendLine("</form>");
            
            return formBuilder.ToString();
        }

        private void UpdateInventoryAndEmergencyNeeds(UserOrder order)
        {
            try
            {
                // 根據訂單類型更新相應的庫存和需求
                if (order.OrderSource == "emergency" && order.EmergencyNeedId.HasValue)
                {
                    // 更新緊急需求
                    var emergencyRecord = _context.EmergencyPurchaseRecords
                        .FirstOrDefault(e => e.UserOrderId == order.UserOrderId);
                    
                    if (emergencyRecord != null)
                    {
                        _context.Database.ExecuteSqlRaw(
                            "UPDATE EmergencySupplyNeeds SET CollectedQuantity = CollectedQuantity + {0}, UpdatedDate = {1} WHERE EmergencyNeedId = {2}",
                            emergencyRecord.Quantity, DateTime.Now, emergencyRecord.EmergencyNeedId);
                        
                        // AsNoTracking 確保從 DB 重新讀取，而非 EF Core 快取的舊值（raw SQL 不會更新 EF 追蹤物件）
                        var emergencyNeed = _context.EmergencySupplyNeeds
                            .AsNoTracking()
                            .FirstOrDefault(e => e.EmergencyNeedId == emergencyRecord.EmergencyNeedId);
                        
                        if (emergencyNeed != null && emergencyNeed.CollectedQuantity >= emergencyNeed.Quantity && emergencyNeed.Status == "Fundraising")
                        {
                            _context.Database.ExecuteSqlRaw(
                                "UPDATE EmergencySupplyNeeds SET Status = 'Completed' WHERE EmergencyNeedId = {0}",
                                emergencyRecord.EmergencyNeedId);
                        }
                    }
                }
                else
                {
                    // 更新物資庫存
                    var orderDetails = _context.UserOrderDetails
                        .Where(od => od.UserOrderId == order.UserOrderId)
                        .ToList();
                    
                    foreach (var detail in orderDetails)
                    {
                        var supply = _context.Supplies.FirstOrDefault(s => s.SupplyId == detail.SupplyId);
                        if (supply != null)
                        {
                            supply.SupplyQuantity += detail.Quantity;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常，避免影響回調處理
                // 可以考慮記錄到日誌系統
            }
        }
    }
}