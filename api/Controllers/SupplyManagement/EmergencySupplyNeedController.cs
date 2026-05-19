using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers.SupplyManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencySupplyNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<EmergencySupplyNeedController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public EmergencySupplyNeedController(NgoplatformDbContext context, ILogger<EmergencySupplyNeedController> logger, IConfiguration configuration, IFileStorageService fileStorageService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// 獲取所有緊急物資需求
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmergencySupplyNeedResponse>>> GetEmergencySupplyNeeds()
        {
            try
            {
                _logger.LogInformation("開始獲取緊急物資需求列表");

                var emergencyNeedsData = await _context.EmergencySupplyNeeds
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.EmergencyNeedId,
                        e.CaseId,
                        e.WorkerId,
                        e.Quantity,
                        e.CollectedQuantity,
                        e.SupplyName,
                        e.Status,
                        e.Priority,
                        e.Description,
                        e.ImageUrl,
                        e.CreatedDate,
                        e.UpdatedDate,
                        CaseName = e.Case != null ? e.Case.Name : null,
                        WorkerName = e.Worker != null ? e.Worker.Name : null
                    })
                    .ToListAsync();

                var response = emergencyNeedsData.Select(e => new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = e.EmergencyNeedId,
                    ItemName = e.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = e.Quantity ?? 0,
                    CollectedQuantity = e.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = e.WorkerName ?? "未知申請人",
                    RequestDate = e.CreatedDate ?? DateTime.Now,
                    Status = e.Status ?? "pending",
                    Priority = e.Priority ?? "medium",
                    Description = e.Description ?? "",
                    ImageUrl = e.ImageUrl ?? "",
                    CaseName = e.CaseName ?? "未知個案",
                    CaseId = e.CaseId?.ToString() ?? "未知",
                    Matched = e.Status == "approved",
                    EmergencyReason = e.Description ?? "緊急物資需求"
                }).ToList();

                _logger.LogInformation($"成功獲取 {response.Count} 個緊急物資需求");
                return Ok(ApiResponse<IEnumerable<EmergencySupplyNeedResponse>>.SuccessResponse(response, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("獲取緊急物資需求失敗", ex.Message));
            }
        }

        /// <summary>
        /// 獲取緊急物資需求統計
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<EmergencySupplyNeedStatistics>> GetEmergencySupplyNeedStatistics()
        {
            try
            {
                _logger.LogInformation("開始獲取緊急物資需求統計");

                var emergencyNeeds = await _context.EmergencySupplyNeeds
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.EmergencyNeedId,
                        e.Quantity,
                        e.CollectedQuantity,
                        e.Status,
                        e.Priority
                    })
                    .ToListAsync();

                var statistics = new EmergencySupplyNeedStatistics
                {
                    TotalRequests = emergencyNeeds.Count,
                    PendingRequests = emergencyNeeds.Count(e => e.Status == "pending"),
                    ApprovedRequests = emergencyNeeds.Count(e => e.Status == "approved"),
                    RejectedRequests = emergencyNeeds.Count(e => e.Status == "rejected"),
                    CompletedRequests = emergencyNeeds.Count(e => e.Status == "completed"),
                    HighPriorityRequests = emergencyNeeds.Count(e => e.Priority == "high"),
                    TotalQuantity = emergencyNeeds.Sum(e => e.Quantity ?? 0),
                    CollectedQuantity = emergencyNeeds.Sum(e => e.CollectedQuantity ?? 0)
                };

                _logger.LogInformation("成功獲取緊急物資需求統計");
                return Ok(ApiResponse<EmergencySupplyNeedStatistics>.SuccessResponse(statistics, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求統計失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("獲取緊急物資需求統計失敗", ex.Message));
            }
        }

        /// <summary>
        /// 根據ID獲取緊急物資需求
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> GetEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Worker)
                    .FirstOrDefaultAsync(e => e.EmergencyNeedId == id);

                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的緊急物資需求"));
                }

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    CollectedQuantity = emergencyNeed.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.CreatedDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    Priority = emergencyNeed.Priority ?? "medium",
                    Description = emergencyNeed.Description ?? "",
                    ImageUrl = emergencyNeed.ImageUrl ?? "",
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = emergencyNeed.Description ?? "緊急物資需求"
                };

                _logger.LogInformation($"成功獲取緊急物資需求 ID: {id}");
                return Ok(ApiResponse<EmergencySupplyNeedResponse>.SuccessResponse(response, "查詢成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("獲取緊急物資需求失敗", ex.Message));
            }
        }

        /// <summary>
        /// 創建緊急物資需求
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> CreateEmergencySupplyNeed([FromBody] CreateEmergencySupplyNeedRequest request)
        {
            try
            {
                _logger.LogInformation("開始創建緊急物資需求");

                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("請求資料不能為空"));

                var emergencyNeed = new EmergencySupplyNeed
                {
                    CaseId = request.CaseId,
                    WorkerId = request.WorkerId,
                    SupplyName = request.SupplyName,
                    Quantity = request.Quantity,
                    CollectedQuantity = 0,
                    Status = request.Status ?? "pending",
                    Priority = request.Priority ?? "medium",
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.EmergencySupplyNeeds.Add(emergencyNeed);
                await _context.SaveChangesAsync();

                await _context.Entry(emergencyNeed).Reference(e => e.Case).LoadAsync();
                await _context.Entry(emergencyNeed).Reference(e => e.Worker).LoadAsync();

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    CollectedQuantity = emergencyNeed.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.CreatedDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    Priority = emergencyNeed.Priority ?? "medium",
                    Description = emergencyNeed.Description ?? "",
                    ImageUrl = emergencyNeed.ImageUrl ?? "",
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = emergencyNeed.Description ?? "緊急物資需求"
                };

                _logger.LogInformation($"成功創建緊急物資需求 ID: {emergencyNeed.EmergencyNeedId}");
                return CreatedAtAction(nameof(GetEmergencySupplyNeed), new { id = emergencyNeed.EmergencyNeedId },
                    ApiResponse<EmergencySupplyNeedResponse>.SuccessResponse(response, "緊急物資需求創建成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建緊急物資需求失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("創建緊急物資需求失敗", ex.Message));
            }
        }

        /// <summary>
        /// 上傳緊急物資需求圖片到 Azure Blob Storage
        /// </summary>
        [HttpPost("upload/image")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("請選擇圖片檔案"));

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(ApiResponse<object>.ErrorResponse("只支援 JPG、PNG、GIF 格式的圖片"));

                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(ApiResponse<object>.ErrorResponse("圖片檔案大小不能超過 5MB"));

                var imageUrl = await _fileStorageService.UploadImageAsync(file);
                _logger.LogInformation($"緊急物資需求圖片上傳成功: {imageUrl}");

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "緊急物資需求圖片上傳失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("緊急物資需求圖片上傳失敗", ex.Message));
            }
        }

        /// <summary>
        /// 批准緊急物資需求
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<ActionResult> ApproveEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始批准緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的緊急物資需求"));
                }

                emergencyNeed.Status = "Fundraising";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功批准緊急物資需求 ID: {id}");
                return Ok(ApiResponse<object>.SuccessResponse(null!, "緊急物資需求已批准，開始對外募集"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批准緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("批准緊急物資需求失敗", ex.Message));
            }
        }

        /// <summary>
        /// 拒絕緊急物資需求
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<ActionResult> RejectEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始拒絕緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的緊急物資需求"));
                }

                emergencyNeed.Status = "rejected";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功拒絕緊急物資需求 ID: {id}");
                return Ok(ApiResponse<object>.SuccessResponse(null!, "緊急物資需求已拒絕"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"拒絕緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("拒絕緊急物資需求失敗", ex.Message));
            }
        }

        /// <summary>
        /// 更新緊急物資需求（含圖片）
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEmergencySupplyNeed(int id, [FromBody] UpdateEmergencyNeedRequest request)
        {
            try
            {
                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的緊急物資需求"));

                if (request.ImageUrl != null) emergencyNeed.ImageUrl = request.ImageUrl;
                if (request.SupplyName != null) emergencyNeed.SupplyName = request.SupplyName;
                if (request.Description != null) emergencyNeed.Description = request.Description;
                if (request.Quantity.HasValue) emergencyNeed.Quantity = request.Quantity;
                if (request.Priority != null) emergencyNeed.Priority = request.Priority;
                emergencyNeed.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.SuccessResponse(null!, "更新成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新失敗", ex.Message));
            }
        }

        /// <summary>
        /// 刪除緊急物資需求
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的緊急物資需求"));
                }

                _context.EmergencySupplyNeeds.Remove(emergencyNeed);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除緊急物資需求 ID: {id}");
                return Ok(ApiResponse<object>.SuccessResponse(null!, "緊急物資需求已刪除"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除緊急物資需求失敗", ex.Message));
            }
        }
    }

    // DTO 類別
    public class EmergencySupplyNeedResponse
    {
        public int EmergencyNeedId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int CollectedQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public bool Matched { get; set; }
        public string EmergencyReason { get; set; } = string.Empty;
    }

    public class CreateEmergencySupplyNeedRequest
    {
        public int? CaseId { get; set; }
        public int? WorkerId { get; set; }
        public string? SupplyName { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EmergencySupplyNeedStatistics
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int HighPriorityRequests { get; set; }
        public int TotalQuantity { get; set; }
        public int CollectedQuantity { get; set; }
    }

    public class UpdateEmergencyNeedRequest
    {
        public string? ImageUrl { get; set; }
        public string? SupplyName { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public string? Priority { get; set; }
    }
}
