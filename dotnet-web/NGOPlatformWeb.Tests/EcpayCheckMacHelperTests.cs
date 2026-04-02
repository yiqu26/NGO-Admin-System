using NGOPlatformWeb.Services;

namespace NGOPlatformWeb.Tests;

/// <summary>
/// 測試 ECPay CheckMacValue 純計算邏輯。
/// 不依賴 DB / HTTP / DI，直接驗證演算法正確性。
/// </summary>
public class EcpayCheckMacHelperTests
{
    // ECPay 官方測試商店憑證（公開於官方文件）
    private const string TestHashKey = "5294y06JbISpM5x9";
    private const string TestHashIv  = "v77hoKGq4kWxNNIS";

    // --- 格式驗證 ---

    [Fact]
    public void Compute_Returns64CharUpperHex()
    {
        var result = EcpayCheckMacHelper.Compute(
            new Dictionary<string, string> { ["MerchantID"] = "2000132" },
            TestHashKey, TestHashIv);

        Assert.Equal(64, result.Length);
        Assert.Matches("^[0-9A-F]+$", result);
    }

    // --- 冪等性 ---

    [Fact]
    public void Compute_SameInputs_ReturnsSameValue()
    {
        var parameters = new Dictionary<string, string>
        {
            ["MerchantID"]       = "2000132",
            ["MerchantTradeNo"]  = "TEST20260402001",
            ["MerchantTradeDate"]= "2026/04/02 10:00:00",
            ["PaymentType"]      = "aio",
            ["TotalAmount"]      = "500",
            ["TradeDesc"]        = "NGO物資捐贈",
            ["ItemName"]         = "物資捐贈",
            ["ChoosePayment"]    = "ALL",
            ["EncryptType"]      = "1"
        };

        var first  = EcpayCheckMacHelper.Compute(parameters, TestHashKey, TestHashIv);
        var second = EcpayCheckMacHelper.Compute(parameters, TestHashKey, TestHashIv);

        Assert.Equal(first, second);
    }

    // --- 參數排序不影響結果 ---

    [Fact]
    public void Compute_ParameterOrder_DoesNotAffectResult()
    {
        var paramsAbc = new Dictionary<string, string>
        {
            ["ChoosePayment"]    = "ALL",
            ["EncryptType"]      = "1",
            ["ItemName"]         = "物資捐贈",
            ["MerchantID"]       = "2000132",
            ["MerchantTradeDate"]= "2026/04/02 10:00:00",
            ["MerchantTradeNo"]  = "TEST20260402001",
            ["PaymentType"]      = "aio",
            ["TotalAmount"]      = "500",
            ["TradeDesc"]        = "NGO物資捐贈"
        };

        var paramsZyx = new Dictionary<string, string>
        {
            ["TradeDesc"]        = "NGO物資捐贈",
            ["TotalAmount"]      = "500",
            ["PaymentType"]      = "aio",
            ["MerchantTradeNo"]  = "TEST20260402001",
            ["MerchantTradeDate"]= "2026/04/02 10:00:00",
            ["MerchantID"]       = "2000132",
            ["ItemName"]         = "物資捐贈",
            ["EncryptType"]      = "1",
            ["ChoosePayment"]    = "ALL"
        };

        var hashAbc = EcpayCheckMacHelper.Compute(paramsAbc, TestHashKey, TestHashIv);
        var hashZyx = EcpayCheckMacHelper.Compute(paramsZyx, TestHashKey, TestHashIv);

        Assert.Equal(hashAbc, hashZyx);
    }

    // --- 空值參數應被排除（付款請求模式） ---

    [Fact]
    public void Compute_EmptyValueParams_ExcludedWhenFlagTrue()
    {
        var withEmpty = new Dictionary<string, string>
        {
            ["MerchantID"]   = "2000132",
            ["TotalAmount"]  = "500",
            ["CreditInstallment"] = ""   // ECPay 要求此欄位未填時不得送出
        };

        var withoutEmpty = new Dictionary<string, string>
        {
            ["MerchantID"]  = "2000132",
            ["TotalAmount"] = "500"
        };

        var hashWith    = EcpayCheckMacHelper.Compute(withEmpty,    TestHashKey, TestHashIv, excludeEmpty: true);
        var hashWithout = EcpayCheckMacHelper.Compute(withoutEmpty, TestHashKey, TestHashIv, excludeEmpty: true);

        Assert.Equal(hashWith, hashWithout);
    }

    // --- 回調模式：空值欄位應保留（excludeEmpty: false） ---

    [Fact]
    public void Compute_EmptyValueParams_RetainedWhenFlagFalse()
    {
        var withEmpty = new Dictionary<string, string>
        {
            ["MerchantID"]   = "2000132",
            ["TotalAmount"]  = "500",
            ["CustomField1"] = ""
        };

        var withoutEmpty = new Dictionary<string, string>
        {
            ["MerchantID"]  = "2000132",
            ["TotalAmount"] = "500"
        };

        var hashWith    = EcpayCheckMacHelper.Compute(withEmpty,    TestHashKey, TestHashIv, excludeEmpty: false);
        var hashWithout = EcpayCheckMacHelper.Compute(withoutEmpty, TestHashKey, TestHashIv, excludeEmpty: false);

        // 保留空值與不保留空值的結果不同，確認 excludeEmpty=false 確實保留了空欄位
        Assert.NotEqual(hashWith, hashWithout);
    }

    // --- CheckMacValue 自身不應被納入計算 ---

    [Fact]
    public void Compute_CheckMacValueKey_IsIgnored()
    {
        var clean = new Dictionary<string, string>
        {
            ["MerchantID"]  = "2000132",
            ["TotalAmount"] = "500"
        };

        var withSelf = new Dictionary<string, string>
        {
            ["MerchantID"]     = "2000132",
            ["TotalAmount"]    = "500",
            ["CheckMacValue"]  = "SOMEPREVIOUSVALUE"  // 不應影響計算
        };

        var hashClean    = EcpayCheckMacHelper.Compute(clean,    TestHashKey, TestHashIv);
        var hashWithSelf = EcpayCheckMacHelper.Compute(withSelf, TestHashKey, TestHashIv);

        Assert.Equal(hashClean, hashWithSelf);
    }

    // --- 不同金額應產生不同 hash（基本敏感度驗證） ---

    [Fact]
    public void Compute_DifferentAmount_ProducesDifferentHash()
    {
        var params500 = new Dictionary<string, string>
        {
            ["MerchantID"]  = "2000132",
            ["TotalAmount"] = "500"
        };

        var params600 = new Dictionary<string, string>
        {
            ["MerchantID"]  = "2000132",
            ["TotalAmount"] = "600"
        };

        var hash500 = EcpayCheckMacHelper.Compute(params500, TestHashKey, TestHashIv);
        var hash600 = EcpayCheckMacHelper.Compute(params600, TestHashKey, TestHashIv);

        Assert.NotEqual(hash500, hash600);
    }
}
