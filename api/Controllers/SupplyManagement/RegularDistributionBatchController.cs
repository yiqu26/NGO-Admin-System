using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;

namespace NGO_WebAPI_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegularDistributionBatchController : ControllerBase
{
    private readonly NgoplatformDbContext _context;

    public RegularDistributionBatchController(NgoplatformDbContext context)
    {
        _context = context;
    }

    // 获取所有分发批次 (根據登入者權限過濾)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDistributionBatches([FromQuery] int? workerId = null)
    {
        try
        {
            IQueryable<RegularDistributionBatch> query = _context.RegularDistributionBatches;

            if (workerId.HasValue)
            {
                query = query.Where(b => _context.RegularSuppliesNeeds
                    .Any(n => n.BatchId == b.DistributionBatchId &&
                              n.Case != null &&
                              n.Case.WorkerId == workerId.Value));
            }

            var batches = await query
                .OrderByDescending(b => b.DistributionDate)
                .ToListAsync();

            var result = batches.Select(b => new
            {
                distributionBatchId = b.DistributionBatchId,
                distributionDate = b.DistributionDate.ToString("yyyy-MM-dd"),
                caseCount = b.CaseCount,
                totalSupplyItems = b.TotalSupplyItems,
                status = b.Status ?? "pending",
                createdAt = b.CreatedAt.HasValue ? b.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                approvedAt = b.ApprovedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                notes = b.Notes,
                createdByWorker = "系統管理員",
                approvedByWorker = b.ApprovedAt != null ? "系統管理員" : null,
                matchCount = 0
            }).ToList();

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(result, "查詢成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法獲取分發批次資料", ex.Message));
        }
    }

    // 获取单个分发批次详情
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDistributionBatch(int id)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches
                .Where(b => b.DistributionBatchId == id)
                .FirstOrDefaultAsync();

            if (batch == null)
                return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的分發批次"));

            var result = new
            {
                distributionBatchId = batch.DistributionBatchId,
                distributionDate = batch.DistributionDate.ToString("yyyy-MM-dd"),
                caseCount = batch.CaseCount,
                totalSupplyItems = batch.TotalSupplyItems,
                status = batch.Status ?? "pending",
                createdAt = batch.CreatedAt.HasValue ? batch.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                approvedAt = batch.ApprovedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                notes = batch.Notes,
                createdByWorker = "系統管理員",
                approvedByWorker = batch.ApprovedAt != null ? "系統管理員" : null,
                matchCount = 0,
                matches = new List<object>()
            };

            return Ok(ApiResponse<object>.SuccessResponse(result, "查詢成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法獲取分發批次詳情", ex.Message));
        }
    }

    // 创建新的分发批次
    [HttpPost]
    public async Task<ActionResult<object>> CreateDistributionBatch([FromBody] CreateDistributionBatchRequest request)
    {
        try
        {
            var batch = new RegularDistributionBatch
            {
                DistributionDate = request.DistributionDate,
                CaseCount = request.CaseCount,
                TotalSupplyItems = request.TotalSupplyItems,
                CreatedByWorkerId = request.CreatedByWorkerId,
                Status = "pending",
                Notes = request.Notes
            };

            _context.RegularDistributionBatches.Add(batch);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { id = batch.DistributionBatchId }, "分發批次創建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法創建分發批次", ex.Message));
        }
    }

    // 批准分发批次
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<object>> ApproveDistributionBatch(int id, [FromBody] ApproveDistributionBatchRequest request)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
                return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的分發批次"));

            batch.Status = "approved";
            batch.ApprovedAt = DateTime.Now;
            batch.ApprovedByWorkerId = request.ApprovedByWorkerId;

            _context.Entry(batch).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null!, "分發批次批准成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法批准分發批次", ex.Message));
        }
    }

    // 拒絕分發批次
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<object>> RejectDistributionBatch(int id, [FromBody] RejectDistributionBatchRequest request)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
                return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的分發批次"));

            batch.Status = "rejected";
            batch.ApprovedAt = DateTime.Now;
            batch.ApprovedByWorkerId = request.RejectedByWorkerId;
            batch.Notes = request.RejectReason ?? "主管拒絕";

            var needsInBatch = await _context.RegularSuppliesNeeds
                .Where(n => n.BatchId == id)
                .ToListAsync();

            foreach (var need in needsInBatch)
            {
                need.Status = "approved";
                need.BatchId = null;
                _context.Entry(need).State = EntityState.Modified;
            }

            _context.Entry(batch).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(
                new { affectedRequests = needsInBatch.Count },
                $"分發批次已拒絕，批次內的 {needsInBatch.Count} 個物資需求已重新設為等待處理狀態"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法拒絕分發批次", ex.Message));
        }
    }

    // 删除分发批次
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDistributionBatch(int id)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
                return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的分發批次"));

            _context.RegularDistributionBatches.Remove(batch);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null!, "分發批次刪除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("無法刪除分發批次", ex.Message));
        }
    }
}

// 請求模型
public class CreateDistributionBatchRequest
{
    public DateTime DistributionDate { get; set; }
    public int CaseCount { get; set; }
    public int TotalSupplyItems { get; set; }
    public int CreatedByWorkerId { get; set; }
    public string? Notes { get; set; }
    public int[]? MatchIds { get; set; }
}

public class ApproveDistributionBatchRequest
{
    public int ApprovedByWorkerId { get; set; }
}

public class RejectDistributionBatchRequest
{
    public int RejectedByWorkerId { get; set; }
    public string? RejectReason { get; set; }
}
