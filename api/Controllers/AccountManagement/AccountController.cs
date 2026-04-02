using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;
using System.Security.Cryptography;
using System.Text;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(NgoplatformDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // DTO Models for Account Management
        public class AccountDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = "staff";
            public string LoginSource { get; set; } = "database";
            public string Status { get; set; } = "active";
            public string CreatedAt { get; set; } = string.Empty;
            public int? WorkerId { get; set; }
            public string? Phone { get; set; }
        }

        public class CreateAccountRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Password { get; set; }
            public string Role { get; set; } = "staff";
            public string LoginSource { get; set; } = "database";
            public string? Phone { get; set; }
        }

        public class UpdateAccountRequest
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
            public string? Status { get; set; }
            public string? Phone { get; set; }
        }

        public class AccountStatsDto
        {
            public int TotalAccounts { get; set; }
            public int ActiveAccounts { get; set; }
            public int InactiveAccounts { get; set; }
            public int AdminCount { get; set; }
            public int SupervisorCount { get; set; }
            public int StaffCount { get; set; }
            public int AzureAccounts { get; set; }
            public int LocalAccounts { get; set; }
        }

        // GET: api/Account
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            try
            {
                _logger.LogInformation("Getting all accounts");

                var workers = await _context.Workers.ToListAsync();

                if (!workers.Any())
                {
                    _logger.LogInformation("No accounts found");
                    return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResponse(new List<AccountDto>(), "查詢成功"));
                }

                var accounts = workers.Select(w => new AccountDto
                {
                    Id = w.WorkerId,
                    Name = w.Name ?? string.Empty,
                    Email = w.Email ?? string.Empty,
                    Role = w.Role ?? "staff",
                    LoginSource = "database",
                    Status = "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = w.WorkerId,
                    Phone = null
                }).ToList();

                _logger.LogInformation($"Retrieved {accounts.Count} accounts");
                return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResponse(accounts, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得帳號列表時發生錯誤", ex.Message));
            }
        }

        // GET: api/Account/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int id)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(id);

                if (worker == null)
                {
                    _logger.LogWarning($"Account with ID {id} not found");
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));
                }

                var account = new AccountDto
                {
                    Id = worker.WorkerId,
                    Name = worker.Name ?? string.Empty,
                    Email = worker.Email ?? string.Empty,
                    Role = worker.Role ?? "staff",
                    LoginSource = "database",
                    Status = "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = worker.WorkerId,
                    Phone = null
                };

                return Ok(ApiResponse<AccountDto>.SuccessResponse(account, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得帳號時發生錯誤", ex.Message));
            }
        }

        // POST: api/Account
        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating new account for email: {request.Email}");

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(ApiResponse<object>.ErrorResponse("姓名為必填欄位"));

                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(ApiResponse<object>.ErrorResponse("電子信箱為必填欄位"));

                var existingWorker = await _context.Workers
                    .FirstOrDefaultAsync(w => w.Email == request.Email);

                if (existingWorker != null)
                    return Conflict(ApiResponse<object>.ErrorResponse("此電子信箱已被使用"));

                if (request.LoginSource == "database" && string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(ApiResponse<object>.ErrorResponse("本地帳戶需要設定密碼"));

                var worker = new Worker
                {
                    Name = request.Name,
                    Email = request.Email,
                    Role = request.Role,
                    Password = request.LoginSource == "database" ? request.Password : null
                };

                _context.Workers.Add(worker);
                await _context.SaveChangesAsync();

                var account = new AccountDto
                {
                    Id = worker.WorkerId,
                    Name = worker.Name,
                    Email = worker.Email,
                    Role = worker.Role ?? "staff",
                    LoginSource = request.LoginSource,
                    Status = "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = worker.WorkerId,
                    Phone = request.Phone
                };

                _logger.LogInformation($"Successfully created account with ID: {worker.WorkerId}");
                return CreatedAtAction(nameof(GetAccount), new { id = worker.WorkerId },
                    ApiResponse<AccountDto>.SuccessResponse(account, "帳號建立成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("建立帳號時發生錯誤", ex.Message));
            }
        }

        // PUT: api/Account/5
        [HttpPut("{id}")]
        public async Task<ActionResult<AccountDto>> UpdateAccount(int id, UpdateAccountRequest request)
        {
            try
            {
                _logger.LogInformation($"Updating account {id}");

                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));

                if (!string.IsNullOrWhiteSpace(request.Name))
                    worker.Name = request.Name;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var existingWorker = await _context.Workers
                        .FirstOrDefaultAsync(w => w.Email == request.Email && w.WorkerId != id);

                    if (existingWorker != null)
                        return Conflict(ApiResponse<object>.ErrorResponse("此電子信箱已被其他帳號使用"));

                    worker.Email = request.Email;
                }

                if (!string.IsNullOrWhiteSpace(request.Role))
                    worker.Role = request.Role;

                await _context.SaveChangesAsync();

                var account = new AccountDto
                {
                    Id = worker.WorkerId,
                    Name = worker.Name ?? string.Empty,
                    Email = worker.Email ?? string.Empty,
                    Role = worker.Role ?? "staff",
                    LoginSource = "database",
                    Status = request.Status ?? "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = worker.WorkerId,
                    Phone = request.Phone
                };

                _logger.LogInformation($"Successfully updated account {id}");
                return Ok(ApiResponse<AccountDto>.SuccessResponse(account, "帳號更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新帳號時發生錯誤", ex.Message));
            }
        }

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting account {id}");

                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));

                var hasActivities = await _context.Activities.AnyAsync(a => a.WorkerId == id);
                var hasCases = await _context.Cases.AnyAsync(c => c.WorkerId == id);
                var hasSchedules = await _context.Schedules.AnyAsync(s => s.WorkerId == id);

                if (hasActivities || hasCases || hasSchedules)
                    return BadRequest(ApiResponse<object>.ErrorResponse("此帳號仍有相關聯的資料，無法刪除。請先處理相關的活動、個案或排程記錄。"));

                _context.Workers.Remove(worker);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted account {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除帳號時發生錯誤", ex.Message));
            }
        }

        // PATCH: api/Account/5/activate
        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<AccountDto>> ActivateAccount(int id)
        {
            try
            {
                _logger.LogInformation($"Activating account {id}");

                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));

                var account = new AccountDto
                {
                    Id = worker.WorkerId,
                    Name = worker.Name ?? string.Empty,
                    Email = worker.Email ?? string.Empty,
                    Role = worker.Role ?? "staff",
                    LoginSource = "database",
                    Status = "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = worker.WorkerId
                };

                _logger.LogInformation($"Successfully activated account {id}");
                return Ok(ApiResponse<AccountDto>.SuccessResponse(account, "帳號已啟用"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("啟用帳號時發生錯誤", ex.Message));
            }
        }

        // PATCH: api/Account/5/deactivate
        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<AccountDto>> DeactivateAccount(int id)
        {
            try
            {
                _logger.LogInformation($"Deactivating account {id}");

                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));

                var account = new AccountDto
                {
                    Id = worker.WorkerId,
                    Name = worker.Name ?? string.Empty,
                    Email = worker.Email ?? string.Empty,
                    Role = worker.Role ?? "staff",
                    LoginSource = "database",
                    Status = "inactive",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    WorkerId = worker.WorkerId
                };

                _logger.LogInformation($"Successfully deactivated account {id}");
                return Ok(ApiResponse<AccountDto>.SuccessResponse(account, "帳號已停用"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("停用帳號時發生錯誤", ex.Message));
            }
        }

        // PATCH: api/Account/5/reset-password
        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation($"Resetting password for account {id}");

                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"找不到ID為 {id} 的帳號"));

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest(ApiResponse<object>.ErrorResponse("新密碼不能為空"));

                if (request.NewPassword.Length < 6)
                    return BadRequest(ApiResponse<object>.ErrorResponse("密碼長度至少需要6個字符"));

                worker.Password = request.NewPassword;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully reset password for account {id}");
                return Ok(ApiResponse<object>.SuccessResponse(null!, "密碼重置成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for account {id}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("重置密碼時發生錯誤", ex.Message));
            }
        }

        // GET: api/Account/check-email
        [HttpGet("check-email")]
        public async Task<ActionResult<CheckEmailResponse>> CheckEmailExists([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(ApiResponse<object>.ErrorResponse("電子信箱參數為必填"));

                var exists = await _context.Workers.AnyAsync(w => w.Email == email);
                return Ok(ApiResponse<CheckEmailResponse>.SuccessResponse(new CheckEmailResponse { Exists = exists }, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("檢查電子信箱時發生錯誤", ex.Message));
            }
        }

        // GET: api/Account/stats
        [HttpGet("stats")]
        public async Task<ActionResult<AccountStatsDto>> GetAccountStats()
        {
            try
            {
                var workers = await _context.Workers.ToListAsync();

                var stats = new AccountStatsDto
                {
                    TotalAccounts = workers.Count,
                    ActiveAccounts = workers.Count,
                    InactiveAccounts = 0,
                    AdminCount = workers.Count(w => w.Role == "admin"),
                    SupervisorCount = workers.Count(w => w.Role == "supervisor"),
                    StaffCount = workers.Count(w => w.Role == "staff"),
                    AzureAccounts = 0,
                    LocalAccounts = workers.Count
                };

                return Ok(ApiResponse<AccountStatsDto>.SuccessResponse(stats, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account statistics");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得帳號統計時發生錯誤", ex.Message));
            }
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Helper classes
        public class ResetPasswordRequest
        {
            public string NewPassword { get; set; } = string.Empty;
        }

        public class CheckEmailResponse
        {
            public bool Exists { get; set; }
        }
    }
}
