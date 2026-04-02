using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;

namespace NGO_WebAPI_Backend.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegularSuppliesNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public RegularSuppliesNeedController(NgoplatformDbContext context)
        {
            _context = context;
        }

        private static string ConvertStatusToEnglish(string status)
        {
            return status switch
            {
                "待審核" => "pending",
                "批准" => "approved",
                "approved" => "approved",
                "不批准" => "rejected",
                "rejected" => "rejected",
                "未領取" => "approved",
                "已領取" => "collected",
                "collected" => "collected",
                "completed" => "collected",
                "等待主管審核" => "pending_super",
                "pending_super" => "pending_super",
                _ => "pending"
            };
        }

        private bool IsPendingStatus(string status) =>
            status == "pending" || status == "待審核";

        private bool IsApprovedStatus(string status) =>
            status == "approved" || status == "批准" || status == "未領取";

        private bool IsRejectedStatus(string status) =>
            status == "rejected" || status == "不批准";

        private bool IsPendingSuperApprovalStatus(string status) =>
            status == "pending_super" || status == "等待主管審核";

        // GET: api/RegularSuppliesNeed
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRegularSuppliesNeeds([FromQuery] int? workerId = null)
        {
            try
            {
                IQueryable<RegularSuppliesNeed> query = _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .ThenInclude(c => c!.Worker)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory);

                if (workerId.HasValue)
                    query = query.Where(r => r.Case != null && r.Case.WorkerId == workerId.Value);

                var needs = await query
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        caseId = r.CaseId,
                        caseName = r.Case != null ? r.Case.Name : "未知",
                        assignedWorkerId = r.Case != null ? r.Case.WorkerId : null,
                        assignedWorkerName = r.Case != null && r.Case.Worker != null ? r.Case.Worker.Name : "未分配",
                        supplyId = r.SupplyId,
                        itemName = r.Supply != null ? r.Supply.SupplyName : "未知",
                        category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName : "未分類",
                        quantity = r.Quantity ?? 0,
                        unit = "個",
                        requestedBy = r.Case != null ? r.Case.Name : "未知",
                        requestDate = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy-MM-dd") : "",
                        status = ConvertStatusToEnglish(r.Status ?? "pending"),
                        estimatedCost = r.Supply != null ? (r.Quantity ?? 0) * (r.Supply.SupplyPrice ?? 0) : 0,
                        deliveryMethod = "自取",
                        pickupDate = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy-MM-dd") : null,
                        matched = false
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(needs, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得常駐物資需求失敗", ex.Message));
            }
        }

        // GET: api/RegularSuppliesNeed/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Where(r => r.RegularNeedId == id)
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        caseId = r.CaseId,
                        caseName = r.Case != null ? r.Case.Name : "未知",
                        supplyId = r.SupplyId,
                        itemName = r.Supply != null ? r.Supply.SupplyName : "未知",
                        category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName : "未分類",
                        quantity = r.Quantity ?? 0,
                        unit = "個",
                        requestedBy = r.Case != null ? r.Case.Name : "未知",
                        requestDate = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy-MM-dd") : "",
                        status = r.Status ?? "pending",
                        estimatedCost = r.Supply != null ? (r.Quantity ?? 0) * (r.Supply.SupplyPrice ?? 0) : 0,
                        deliveryMethod = "自取",
                        pickupDate = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy-MM-dd") : null,
                        matched = false
                    })
                    .FirstOrDefaultAsync();

                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                return Ok(ApiResponse<object>.SuccessResponse(need, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得常駐物資需求失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed
        [HttpPost]
        public async Task<ActionResult<object>> PostRegularSuppliesNeed([FromBody] CreateRegularSuppliesNeedRequest request)
        {
            try
            {
                var need = new RegularSuppliesNeed
                {
                    CaseId = request.CaseId,
                    SupplyId = request.SupplyId,
                    Quantity = request.Quantity,
                    ApplyDate = DateTime.Now,
                    Status = "pending"
                };

                _context.RegularSuppliesNeeds.Add(need);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRegularSuppliesNeed), new { id = need.RegularNeedId },
                    ApiResponse<object>.SuccessResponse(new { needId = need.RegularNeedId }, "常駐物資需求新增成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("新增常駐物資需求失敗", ex.Message));
            }
        }

        // PUT: api/RegularSuppliesNeed/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegularSuppliesNeed(int id, [FromBody] UpdateRegularSuppliesNeedRequest request)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                need.CaseId = request.CaseId ?? need.CaseId;
                need.SupplyId = request.SupplyId ?? need.SupplyId;
                need.Quantity = request.Quantity ?? need.Quantity;
                need.Status = request.Status ?? need.Status;
                need.PickupDate = request.PickupDate ?? need.PickupDate;

                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "常駐物資需求更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新常駐物資需求失敗", ex.Message));
            }
        }

        // DELETE: api/RegularSuppliesNeed/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                _context.RegularSuppliesNeeds.Remove(need);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "常駐物資需求刪除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除常駐物資需求失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                need.Status = "approved";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "常駐物資需求批准成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("批准常駐物資需求失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                need.Status = "rejected";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "常駐物資需求拒絕成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("拒絕常駐物資需求失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/confirm
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                if (!IsApprovedStatus(need.Status ?? ""))
                    return BadRequest(ApiResponse<object>.ErrorResponse("只有已批准的申請可以確認"));

                need.Status = "pending_super";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "物資需求確認成功，已轉入主管審核"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("確認物資需求失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/supervisor-approve
        [HttpPost("{id}/supervisor-approve")]
        public async Task<IActionResult> SupervisorApproveRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                if (!IsPendingSuperApprovalStatus(need.Status ?? ""))
                    return BadRequest(ApiResponse<object>.ErrorResponse("只有等待主管審核的申請可以批准"));

                need.Status = "collected";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "主管批准成功，物資已發放"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("主管批准失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/supervisor-reject
        [HttpPost("{id}/supervisor-reject")]
        public async Task<IActionResult> SupervisorRejectRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                if (!IsPendingSuperApprovalStatus(need.Status ?? ""))
                    return BadRequest(ApiResponse<object>.ErrorResponse("只有等待主管審核的申請可以拒絕"));

                need.Status = "rejected";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "主管拒絕成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("主管拒絕失敗", ex.Message));
            }
        }

        // POST: api/RegularSuppliesNeed/5/collect
        [HttpPost("{id}/collect")]
        public async Task<IActionResult> CollectRegularSuppliesNeed(int id, [FromBody] CollectRegularSuppliesNeedRequest? request = null)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的常駐物資需求"));

                need.Status = "collected";
                need.PickupDate = DateTime.Now;
                need.BatchId = request?.BatchId;
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "常駐物資需求標記為已領取成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("標記常駐物資需求為已領取失敗", ex.Message));
            }
        }

        // GET: api/RegularSuppliesNeed/batch/5/details
        [HttpGet("batch/{batchId}/details")]
        public async Task<ActionResult<IEnumerable<object>>> GetBatchDistributionDetails(int batchId)
        {
            try
            {
                var batchDetails = await _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Where(r => r.BatchId == batchId)
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        申請人 = r.Case != null ? r.Case.Name : "未知",
                        物品名稱 = r.Supply != null ? r.Supply.SupplyName : "未知",
                        申請數量 = r.Quantity ?? 0,
                        配對數量 = r.Quantity ?? 0,
                        申請日期 = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy/M/d") : "",
                        配對日期 = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy/M/d") : "",
                        狀態 = r.Status ?? "未知",
                        備註 = "系統管理員"
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(batchDetails, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得批次分發詳情失敗", ex.Message));
            }
        }

        // GET: api/RegularSuppliesNeed/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetRegularSuppliesNeedStats([FromQuery] int? workerId = null)
        {
            try
            {
                IQueryable<RegularSuppliesNeed> query = _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply);

                if (workerId.HasValue)
                    query = query.Where(r => r.Case != null && r.Case.WorkerId == workerId.Value);

                var totalRequests = await query.CountAsync();

                var allRequests = await query
                    .Select(r => r.Status)
                    .ToListAsync();

                var pendingRequests = allRequests.Count(status => IsPendingStatus(status ?? ""));
                var approvedRequests = allRequests.Count(status => IsApprovedStatus(status ?? ""));
                var rejectedRequests = allRequests.Count(status => IsRejectedStatus(status ?? ""));

                var totalEstimatedCost = await query
                    .SumAsync(r => (r.Quantity ?? 0) * (r.Supply != null ? r.Supply.SupplyPrice ?? 0 : 0));

                var stats = new
                {
                    totalRequests,
                    pendingRequests,
                    approvedRequests,
                    rejectedRequests,
                    totalEstimatedCost
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得常駐物資需求統計失敗", ex.Message));
            }
        }
    }

    public class CreateRegularSuppliesNeedRequest
    {
        public int CaseId { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateRegularSuppliesNeedRequest
    {
        public int? CaseId { get; set; }
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public DateTime? PickupDate { get; set; }
    }

    public class CollectRegularSuppliesNeedRequest
    {
        public int? BatchId { get; set; }
    }
}
