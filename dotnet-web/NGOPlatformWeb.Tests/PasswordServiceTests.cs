using NGOPlatformWeb.Services;

namespace NGOPlatformWeb.Tests;

/// <summary>
/// 測試 PasswordService 的 BCrypt hash / verify 邏輯。
/// BCrypt 是系統密碼儲存的核心安全機制，此測試確保行為符合預期。
/// 不依賴 DB / HTTP / DI。
/// </summary>
public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    // --- HashPassword ---

    [Fact]
    public void HashPassword_ReturnsNonEmptyBcryptString()
    {
        var hash = _sut.HashPassword("Test123!");

        Assert.NotEmpty(hash);
        Assert.StartsWith("$2", hash); // BCrypt hash 固定以 $2a$ 或 $2b$ 開頭
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentHashes()
    {
        // BCrypt 每次產生不同 salt，相同密碼應得到不同 hash
        var hash1 = _sut.HashPassword("Test123!");
        var hash2 = _sut.HashPassword("Test123!");

        Assert.NotEqual(hash1, hash2);
    }

    // --- VerifyPassword ---

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("Test123!");

        Assert.True(_sut.VerifyPassword("Test123!", hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Test123!");

        Assert.False(_sut.VerifyPassword("WrongPassword!", hash));
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Test123!");

        Assert.False(_sut.VerifyPassword("", hash));
    }

    // --- 端對端 roundtrip ---

    [Theory]
    [InlineData("Admin123!")]
    [InlineData("short")]
    [InlineData("含中文密碼123")]
    [InlineData("!@#$%^&*()")]
    public void HashThenVerify_Roundtrip_ReturnsTrue(string password)
    {
        var hash = _sut.HashPassword(password);

        Assert.True(_sut.VerifyPassword(password, hash));
    }
}
