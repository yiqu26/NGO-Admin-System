using FluentValidation;
using NGO_WebAPI_Backend.DTOs; using NGO_WebAPI_Backend.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace NGO_WebAPI_Backend.Validators
{
    /// <summary>
    /// 建立個案資料驗證器
    /// </summary>
    public class CreateCaseDtoValidator : AbstractValidator<CreateCaseDto>
    {
        private readonly NgoplatformDbContext _context;

        public CreateCaseDtoValidator(NgoplatformDbContext context)
        {
            _context = context;
            // 姓名驗證
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("姓名不能為空")
                .Length(2, 50).WithMessage("姓名長度必須在 2-50 字元之間")
                .Matches(@"^[\u4e00-\u9fa5a-zA-Z\s]+$").WithMessage("姓名只能包含中文、英文字母和空格");

            // 電話驗證
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("電話號碼不能為空")
                .Matches(@"^09\d{8}$").WithMessage("請輸入正確的手機號碼格式 (09xxxxxxxx)")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            // 身分證字號驗證（格式檢查，不做 checksum 驗證以允許居留證等特殊情形）
            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("身分證字號不能為空")
                .Length(10).WithMessage("身分證字號必須為 10 位數")
                .Matches(@"^[A-Z][0-9]{9}$").WithMessage("身分證字號格式錯誤，應為 1 個英文字母加 9 個數字");

            // 生日驗證
            RuleFor(x => x.Birthday)
                .LessThan(DateTime.Now).WithMessage("生日不能是未來日期")
                .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("生日不能超過 120 年前")
                .When(x => x.Birthday.HasValue);

            // 工作人員ID驗證（暫時簡化，避免 Azure 上的資料庫連接問題）
            RuleFor(x => x.WorkerId)
                .GreaterThan(0).WithMessage("工作人員ID必須大於 0")
                .When(x => x.WorkerId.HasValue);

            // 電子郵件驗證
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("請輸入正確的電子郵件格式")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // 性別驗證
            RuleFor(x => x.Gender)
                .Must(gender => string.IsNullOrEmpty(gender) || 
                               new[] { "Male", "Female", "Other", "male", "female", "other" }.Contains(gender))
                .WithMessage("性別只能是 Male、Female 或 Other");

            // 城市驗證
            RuleFor(x => x.City)
                .MaximumLength(20).WithMessage("城市名稱不能超過 20 個字元")
                .When(x => !string.IsNullOrEmpty(x.City));

            // 區域驗證
            RuleFor(x => x.District)
                .MaximumLength(20).WithMessage("區域名稱不能超過 20 個字元")
                .When(x => !string.IsNullOrEmpty(x.District));

            // 詳細地址驗證
            RuleFor(x => x.DetailAddress)
                .MaximumLength(200).WithMessage("詳細地址不能超過 200 個字元")
                .When(x => !string.IsNullOrEmpty(x.DetailAddress));

            // 描述驗證（困難類別 - 必填）
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("困難類別不能為空")
                .MaximumLength(1000).WithMessage("困難類別描述不能超過 1000 個字元");

            // 個人照片URL驗證
            RuleFor(x => x.ProfileImage)
                .Must(BeValidUrl).WithMessage("個人照片必須是有效的 URL")
                .When(x => !string.IsNullOrEmpty(x.ProfileImage));

            // 語音檔案URL驗證
            RuleFor(x => x.SpeechToTextAudioUrl)
                .Must(BeValidUrl).WithMessage("語音檔案必須是有效的 URL")
                .When(x => !string.IsNullOrEmpty(x.SpeechToTextAudioUrl));
        }

        /// <summary>
        /// 驗證URL格式
        /// </summary>
        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

    }
}