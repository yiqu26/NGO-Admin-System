using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderDetailController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public UserOrderDetailController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/UserOrderDetail/{orderId}
        [HttpGet("{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserOrderDetails(int orderId)
        {
            try
            {
                var orderDetails = await _context.UserOrderDetails
                    .Include(d => d.Supply)
                    .Where(d => d.UserOrderId == orderId)
                    .Select(d => new
                    {
                        orderDetailId = d.DetailId,
                        userOrderId = d.UserOrderId,
                        supplyId = d.SupplyId,
                        supplyName = d.Supply != null ? d.Supply.SupplyName : "未知物資",
                        quantity = d.Quantity ?? 0,
                        unitPrice = d.UnitPrice ?? 0,
                        totalPrice = (d.Quantity ?? 0) * (d.UnitPrice ?? 0)
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(orderDetails, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得用戶訂單詳情失敗", ex.Message));
            }
        }

        // GET: api/UserOrderDetail/detail/{id}
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<object>> GetUserOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails
                    .Include(d => d.Supply)
                    .Where(d => d.DetailId == id)
                    .Select(d => new
                    {
                        orderDetailId = d.DetailId,
                        userOrderId = d.UserOrderId,
                        supplyId = d.SupplyId,
                        supplyName = d.Supply != null ? d.Supply.SupplyName : "未知物資",
                        quantity = d.Quantity ?? 0,
                        unitPrice = d.UnitPrice ?? 0,
                        totalPrice = (d.Quantity ?? 0) * (d.UnitPrice ?? 0)
                    })
                    .FirstOrDefaultAsync();

                if (orderDetail == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的訂單詳情"));

                return Ok(ApiResponse<object>.SuccessResponse(orderDetail, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得訂單詳情失敗", ex.Message));
            }
        }

        // POST: api/UserOrderDetail
        [HttpPost]
        public async Task<ActionResult<object>> PostUserOrderDetail([FromBody] CreateUserOrderDetailRequest request)
        {
            try
            {
                var orderDetail = new UserOrderDetail
                {
                    UserOrderId = request.UserOrderId,
                    SupplyId = request.SupplyId,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice
                };

                _context.UserOrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserOrderDetail), new { id = orderDetail.DetailId },
                    ApiResponse<object>.SuccessResponse(new { detailId = orderDetail.DetailId }, "訂單詳情新增成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("新增訂單詳情失敗", ex.Message));
            }
        }

        // PUT: api/UserOrderDetail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserOrderDetail(int id, [FromBody] UpdateUserOrderDetailRequest request)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails.FindAsync(id);
                if (orderDetail == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的訂單詳情"));

                orderDetail.SupplyId = request.SupplyId ?? orderDetail.SupplyId;
                orderDetail.Quantity = request.Quantity ?? orderDetail.Quantity;
                orderDetail.UnitPrice = request.UnitPrice ?? orderDetail.UnitPrice;

                _context.Entry(orderDetail).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "訂單詳情更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新訂單詳情失敗", ex.Message));
            }
        }

        // DELETE: api/UserOrderDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails.FindAsync(id);
                if (orderDetail == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的訂單詳情"));

                _context.UserOrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "訂單詳情刪除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除訂單詳情失敗", ex.Message));
            }
        }
    }

    // DTO Classes
    public class CreateUserOrderDetailRequest
    {
        public int UserOrderId { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateUserOrderDetailRequest
    {
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}
