using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public UserOrderController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/UserOrder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserOrders()
        {
            try
            {
                var orders = await _context.UserOrders
                    .Select(o => new
                    {
                        userOrderId = o.UserOrderId,
                        userId = o.UserId ?? 0,
                        userName = "用戶",
                        orderDate = o.OrderDate != null ? o.OrderDate.Value.ToString("yyyy-MM-dd") : "",
                        status = o.PaymentStatus ?? "pending",
                        totalAmount = o.TotalPrice ?? 0
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(orders, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得用戶訂單失敗", ex.Message));
            }
        }

        // GET: api/UserOrder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUserOrder(int id)
        {
            try
            {
                var order = await _context.UserOrders
                    .Where(o => o.UserOrderId == id)
                    .Select(o => new
                    {
                        userOrderId = o.UserOrderId,
                        userId = o.UserId ?? 0,
                        userName = "用戶",
                        orderDate = o.OrderDate != null ? o.OrderDate.Value.ToString("yyyy-MM-dd") : "",
                        status = o.PaymentStatus ?? "pending",
                        totalAmount = o.TotalPrice ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的用戶訂單"));

                return Ok(ApiResponse<object>.SuccessResponse(order, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得用戶訂單失敗", ex.Message));
            }
        }

        // POST: api/UserOrder
        [HttpPost]
        public async Task<ActionResult<object>> PostUserOrder([FromBody] CreateUserOrderRequest request)
        {
            try
            {
                var order = new UserOrder
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.Now,
                    PaymentStatus = "pending",
                    TotalPrice = request.TotalAmount,
                    OrderNumber = Guid.NewGuid().ToString()
                };

                _context.UserOrders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserOrder), new { id = order.UserOrderId },
                    ApiResponse<object>.SuccessResponse(new { orderId = order.UserOrderId }, "用戶訂單新增成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("新增用戶訂單失敗", ex.Message));
            }
        }

        // PUT: api/UserOrder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserOrder(int id, [FromBody] UpdateUserOrderRequest request)
        {
            try
            {
                var order = await _context.UserOrders.FindAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的用戶訂單"));

                order.UserId = request.UserId ?? order.UserId;
                order.PaymentStatus = request.Status ?? order.PaymentStatus;
                order.TotalPrice = request.TotalAmount ?? order.TotalPrice;

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "用戶訂單更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新用戶訂單失敗", ex.Message));
            }
        }

        // DELETE: api/UserOrder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserOrder(int id)
        {
            try
            {
                var order = await _context.UserOrders.FindAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的用戶訂單"));

                _context.UserOrders.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "用戶訂單刪除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除用戶訂單失敗", ex.Message));
            }
        }
    }

    // DTO Classes
    public class CreateUserOrderRequest
    {
        public int? UserId { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    public class UpdateUserOrderRequest
    {
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
