using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NGOPlatformWeb.Services
{
    /// <summary>
    /// ECPay CheckMacValue 純計算邏輯，抽離成靜態類別以便單元測試。
    /// </summary>
    public static class EcpayCheckMacHelper
    {
        /// <summary>
        /// 依照 ECPay AIO 規範計算 CheckMacValue（SHA256）。
        /// 演算法：排序 → 組合字串 → UrlEncode+小寫 → 特殊字元還原 → SHA256 大寫十六進位。
        /// </summary>
        /// <param name="parameters">付款參數（不含 CheckMacValue）</param>
        /// <param name="hashKey">ECPay HashKey</param>
        /// <param name="hashIv">ECPay HashIV</param>
        /// <param name="excludeEmpty">是否排除空值參數（付款請求用 true，回調驗證用 false）</param>
        public static string Compute(
            Dictionary<string, string> parameters,
            string hashKey,
            string hashIv,
            bool excludeEmpty = true)
        {
            // 1. 過濾 CheckMacValue 本身；依 excludeEmpty 決定是否排除空值
            var filtered = parameters
                .Where(kv => kv.Key != "CheckMacValue")
                .Where(kv => !excludeEmpty || !string.IsNullOrEmpty(kv.Value));

            // 2. 依 Key 字典序排序
            var sorted = filtered.OrderBy(kv => kv.Key);

            // 3. 組合查詢字串
            var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

            // 4. 加上 HashKey / HashIV
            var checkString = $"HashKey={hashKey}&{queryString}&HashIV={hashIv}";

            // 5. URL 編碼後轉小寫
            var encoded = HttpUtility.UrlEncode(checkString, Encoding.UTF8).ToLower();

            // 6. ECPay 規範：還原不應被編碼的特殊字元
            encoded = encoded
                .Replace("%21", "!")
                .Replace("%2a", "*")
                .Replace("%28", "(")
                .Replace("%29", ")")
                .Replace("%2d", "-")
                .Replace("%5f", "_")
                .Replace("%2e", ".");

            // 7. SHA256 → 大寫十六進位
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(encoded));
            return Convert.ToHexString(hashBytes); // .NET 5+ 內建，已大寫
        }
    }
}
