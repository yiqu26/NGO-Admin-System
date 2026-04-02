using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Shared;

namespace NGO_WebAPI_Backend.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public SupplyController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/Supply
        /// <summary>
        /// 取得所有物資清單
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetSupplies()
        {
            try
            {
                var supplies = await _context.Supplies
                    .Include(s => s.SupplyCategory)
                    .Select(s => new
                    {
                        supplyId = s.SupplyId,
                        name = s.SupplyName,
                        categoryId = s.SupplyCategoryId,
                        categoryName = s.SupplyCategory != null ? s.SupplyCategory.SupplyCategoryName : "未分類",
                        currentStock = s.SupplyQuantity ?? 0,
                        unit = "個",
                        location = "倉庫",
                        supplier = "系統供應商",
                        cost = s.SupplyPrice ?? 0,
                        addedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        expiryDate = (string?)null,
                        description = s.SupplyDescription,
                        supplyType = s.SupplyType ?? "regular"
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(supplies, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得物資清單失敗", ex.Message));
            }
        }

        // GET: api/Supply/5
        /// <summary>
        /// 取得單一物資詳細資訊
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetSupply(int id)
        {
            try
            {
                var supply = await _context.Supplies
                    .Include(s => s.SupplyCategory)
                    .Where(s => s.SupplyId == id)
                    .Select(s => new
                    {
                        supplyId = s.SupplyId,
                        name = s.SupplyName,
                        categoryId = s.SupplyCategoryId,
                        categoryName = s.SupplyCategory != null ? s.SupplyCategory.SupplyCategoryName : "未分類",
                        currentStock = s.SupplyQuantity ?? 0,
                        unit = "個",
                        location = "倉庫",
                        supplier = "系統供應商",
                        cost = s.SupplyPrice ?? 0,
                        addedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        expiryDate = (string?)null,
                        description = s.SupplyDescription,
                        supplyType = s.SupplyType ?? "regular",
                        imageUrl = s.ImageUrl
                    })
                    .FirstOrDefaultAsync();

                if (supply == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的物資"));

                return Ok(ApiResponse<object>.SuccessResponse(supply, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得物資詳細資訊失敗", ex.Message));
            }
        }

        // POST: api/Supply
        /// <summary>
        /// 新增物資
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostSupply([FromBody] CreateSupplyRequest request)
        {
            try
            {
                var supply = new Supply
                {
                    SupplyName = request.Name,
                    SupplyCategoryId = request.CategoryId,
                    SupplyQuantity = request.Quantity,
                    SupplyPrice = request.Price,
                    SupplyDescription = request.Description,
                    SupplyType = request.SupplyType ?? "regular",
                    ImageUrl = request.ImageUrl
                };

                _context.Supplies.Add(supply);
                await _context.SaveChangesAsync();

                var createdSupply = await _context.Supplies
                    .Include(s => s.SupplyCategory)
                    .Where(s => s.SupplyId == supply.SupplyId)
                    .Select(s => new
                    {
                        supplyId = s.SupplyId,
                        name = s.SupplyName,
                        categoryId = s.SupplyCategoryId,
                        categoryName = s.SupplyCategory != null ? s.SupplyCategory.SupplyCategoryName : "未分類",
                        currentStock = s.SupplyQuantity ?? 0,
                        unit = "個",
                        location = "倉庫",
                        supplier = "系統供應商",
                        cost = s.SupplyPrice ?? 0,
                        addedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        expiryDate = (string?)null,
                        description = s.SupplyDescription,
                        supplyType = s.SupplyType ?? "regular"
                    })
                    .FirstOrDefaultAsync();

                return StatusCode(201, ApiResponse<object>.SuccessResponse(createdSupply!, "新增成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("新增物資失敗", ex.Message));
            }
        }

        // PUT: api/Supply/5
        /// <summary>
        /// 更新物資資訊
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutSupply(int id, [FromBody] UpdateSupplyRequest request)
        {
            try
            {
                var supply = await _context.Supplies.FindAsync(id);
                if (supply == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的物資"));

                if (request.Name != null) supply.SupplyName = request.Name;
                if (request.CategoryId.HasValue) supply.SupplyCategoryId = request.CategoryId;
                if (request.Quantity.HasValue) supply.SupplyQuantity = request.Quantity;
                if (request.Price.HasValue) supply.SupplyPrice = request.Price;
                if (request.Description != null) supply.SupplyDescription = request.Description;
                if (request.SupplyType != null) supply.SupplyType = request.SupplyType;
                if (request.ImageUrl != null) supply.ImageUrl = request.ImageUrl;

                _context.Entry(supply).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "物資更新成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("更新物資失敗", ex.Message));
            }
        }

        // DELETE: api/Supply/5
        /// <summary>
        /// 刪除物資
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSupply(int id)
        {
            try
            {
                var supply = await _context.Supplies.FindAsync(id);
                if (supply == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("找不到指定的物資"));

                _context.Supplies.Remove(supply);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null!, "物資刪除成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("刪除物資失敗", ex.Message));
            }
        }

        // GET: api/Supply/categories
        /// <summary>
        /// 取得所有物資分類
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetSupplyCategories()
        {
            try
            {
                var categories = await _context.SupplyCategories
                    .Select(c => new
                    {
                        id = c.SupplyCategoryId,
                        name = c.SupplyCategoryName
                    })
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(categories, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得物資分類失敗", ex.Message));
            }
        }

        // GET: api/Supply/stats
        /// <summary>
        /// 取得物資統計資料
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetSupplyStats()
        {
            try
            {
                var totalItems = await _context.Supplies.CountAsync();
                var lowStockItems = await _context.Supplies
                    .Where(s => s.SupplyQuantity < 10)
                    .CountAsync();
                var totalValue = await _context.Supplies
                    .SumAsync(s => (s.SupplyQuantity ?? 0) * (s.SupplyPrice ?? 0));

                var stats = new
                {
                    totalItems,
                    lowStockItems,
                    totalValue
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "查詢成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("取得物資統計失敗", ex.Message));
            }
        }

        // POST: api/Supply/categories
        /// <summary>
        /// 新增物資分類
        /// </summary>
        [HttpPost("categories")]
        public async Task<ActionResult<ApiResponse<object>>> PostSupplyCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = new SupplyCategory
                {
                    SupplyCategoryName = request.Name
                };

                _context.SupplyCategories.Add(category);
                await _context.SaveChangesAsync();

                var result = new
                {
                    id = category.SupplyCategoryId,
                    name = category.SupplyCategoryName
                };

                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "新增分類成功"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("新增物資分類失敗", ex.Message));
            }
        }
    }

    // DTO Classes
    public class CreateSupplyRequest
    {
        public string Name { get; set; } = null!;
        public int? CategoryId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? SupplyType { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateSupplyRequest
    {
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? SupplyType { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = null!;
    }
}
