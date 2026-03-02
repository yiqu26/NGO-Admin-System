using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Purchase;
using NGOPlatformWeb.Services;

namespace NGOPlatformWeb.Controllers
{
    // 物資認購控制器 - 處理捐贈頁面功能
    public class PurchaseController : Controller
    {
        private readonly NGODbContext _context;
        private readonly EcpayService _ecpayService;
        private readonly IConfiguration _configuration;

        public PurchaseController(NGODbContext context, EcpayService ecpayService, IConfiguration configuration)
        {
            _context = context;
            _ecpayService = ecpayService;
            _configuration = configuration;
        }

        // 主要捐贈頁面 - 顯示緊急需求和常規物資供民眾認購
        public IActionResult Index()
        {
            try
            {
                // 初始化資料容器
                var emergencyNeeds = new List<object>();
                var regularSupplies = new List<object>();

                // 取得緊急需求資料
                try
                {
                    var rawEmergencyNeeds = _context.EmergencySupplyNeeds
                        .Where(e => e.Status == "Fundraising" && e.CollectedQuantity < e.Quantity)
                        .OrderBy(e => e.Priority == "Urgent" ? 1 : 
                                      e.Priority == "High" ? 2 : 
                                      e.Priority == "Normal" ? 3 : 4)
                        .ThenByDescending(e => (decimal)(e.Quantity - e.CollectedQuantity) / e.Quantity) // 剩餘比例高的優先
                        .Take(6) // 限制顯示6個緊急需求項目
                        .ToList();

                    // 轉換為顯示格式，加入 null 值保護
                    emergencyNeeds = rawEmergencyNeeds.Select(e => new 
                        {
                            Id = e.EmergencyNeedId,
                            CaseId = e.CaseId,
                            SupplyName = e.SupplyName ?? "未知物資",
                            NeededQuantity = e.Quantity,
                            RemainingQuantity = Math.Max(0, e.Quantity - e.CollectedQuantity),
                            ImageUrl = !string.IsNullOrEmpty(e.ImageUrl) ? e.ImageUrl : GetDefaultEmergencyImage(e.SupplyName ?? ""),
                            Status = e.Status ?? "未知狀態",
                            Priority = e.Priority ?? "Normal",
                            Description = e.Description ?? "無描述"
                        })
                        .ToList<object>();
                }
                catch (Exception ex)
                {
                    ViewBag.EmergencyError = "緊急需求資料載入失敗: " + ex.Message;
                }

                // 取得常規物資 - 單項捐贈選擇
                try
                {
                    regularSupplies = _context.Supplies
                        .Include(s => s.SupplyCategory)
                        .Where(s => s.SupplyType == "regular")
                        .Take(30) // 顯示30樣常規物資
                        .ToList<object>();
                }
                catch (Exception ex)
                {
                    ViewBag.RegularError = "常規物資資料載入失敗: " + ex.Message;
                }

                // 將資料傳遞給前端視圖
                ViewBag.EmergencyNeeds = emergencyNeeds;
                ViewBag.RegularSupplies = regularSupplies;

                return View();
            }
            catch (Exception ex)
            {
                // 錯誤處理 - 提供空資料避免頁面崩潰
                ViewBag.EmergencyNeeds = new List<object>();
                ViewBag.RegularSupplies = new List<object>();
                ViewBag.Error = "頁面載入發生錯誤: " + ex.Message;
                ViewBag.ErrorDetail = ex.ToString();
                return View();
            }
        }

        // 直接購買單項物資 - 準備付款資料並跳轉到付款頁面
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectPurchase(int? supplyId = null, int quantity = 1, int? emergencyNeedId = null)
        {
            try
            {
                // 如果是緊急需求，處理緊急物資認購
                if (emergencyNeedId.HasValue)
                {
                    var emergencyNeed = _context.EmergencySupplyNeeds
                        .FirstOrDefault(e => e.EmergencyNeedId == emergencyNeedId.Value);
                    if (emergencyNeed == null)
                    {
                        TempData["Error"] = "找不到指定的緊急需求項目";
                        return RedirectToAction("Index");
                    }

                    // 檢查剩餘需求量
                    var remainingQuantity = emergencyNeed.Quantity - emergencyNeed.CollectedQuantity;
                    if (remainingQuantity <= 0)
                    {
                        TempData["Error"] = "此緊急需求已滿足，無需額外認購";
                        return RedirectToAction("Index");
                    }

                    if (quantity > remainingQuantity)
                    {
                        TempData["Error"] = $"認購數量過多，目前還需要 {remainingQuantity} 份";
                        return RedirectToAction("Index");
                    }

                    // 查詢個案描述
                    string? caseDescription = null;
                    var caseEntity = await _context.Cases
                        .FirstOrDefaultAsync(c => c.CaseId == emergencyNeed.CaseId);
                    caseDescription = caseEntity?.Description;

                    // 建立緊急物資付款資料模型 - 根據物資類型設定合理價格
                    decimal unitPrice = GetEmergencySupplyPrice(emergencyNeed.SupplyName ?? "");
                    
                    var paymentModel = new PaymentViewModel
                    {
                        // 緊急物資不需要 SupplyId，完全獨立於物資總表
                        SupplyName = emergencyNeed.SupplyName ?? "未知物資",
                        Quantity = quantity,
                        TotalPrice = unitPrice * quantity, // 根據物資類型計算價格
                        SupplyType = "emergency",
                        EmergencyNeedId = emergencyNeedId,
                        CaseId = emergencyNeed.CaseId,
                        CaseDescription = caseDescription,
                        MaxQuantity = remainingQuantity,
                        IsLoggedIn = User.Identity?.IsAuthenticated ?? false
                    };

                    // 預填已登入用戶資訊
                    if (paymentModel.IsLoggedIn)
                    {
                        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                        
                        if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                        {
                            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                            if (user != null)
                            {
                                paymentModel.UserId = userId;
                                paymentModel.DonorName = user.Name ?? "";
                                paymentModel.DonorEmail = user.Email ?? "";
                                paymentModel.DonorPhone = user.Phone ?? "";
                            }
                        }
                    }

                    return View("Payment", paymentModel);
                }

                // 處理一般物資認購
                if (!supplyId.HasValue)
                {
                    TempData["Error"] = "缺少物資項目資訊";
                    return RedirectToAction("Index");
                }

                var supply = _context.Supplies.FirstOrDefault(s => s.SupplyId == supplyId.Value);
                if (supply == null)
                {
                    TempData["Error"] = "找不到指定的物資項目";
                    return RedirectToAction("Index");
                }

                // 檢查庫存是否足夠
                if (supply.SupplyQuantity < quantity)
                {
                    TempData["Error"] = $"庫存不足，目前只剩 {supply.SupplyQuantity} 份";
                    return RedirectToAction("Index");
                }

                // 建立付款資料模型
                var regularPaymentModel = new PaymentViewModel
                {
                    SupplyId = supply.SupplyId,
                    SupplyName = supply.SupplyName ?? "未知物資",
                    Quantity = quantity,
                    TotalPrice = (supply.SupplyPrice ?? 0) * quantity,
                    SupplyType = supply.SupplyType,
                    IsLoggedIn = User.Identity?.IsAuthenticated ?? false
                };

                // 如果已登入，預填用戶資訊
                if (regularPaymentModel.IsLoggedIn)
                {
                    var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    
                    // 只處理一般用戶(User)，不處理個案(Case)
                    if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            regularPaymentModel.UserId = userId;
                            regularPaymentModel.DonorName = user.Name ?? "";
                            regularPaymentModel.DonorEmail = user.Email ?? "";
                            regularPaymentModel.DonorPhone = user.Phone ?? "";
                        }
                    }
                }

                return View("Payment", regularPaymentModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "處理購買請求時發生錯誤: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // 付款頁面
        public IActionResult Payment()
        {
            return RedirectToAction("Index");
        }

        // 組合包購買 - 根據實際物資價格計算組合包總價
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyPackage(string packageType, int quantity = 1)
        {
            // 根據實際物資價格計算組合包價格
            var (name, price) = packageType switch
            {
                "medical" => ("醫療照護包", 705m), // 80*2+60*2+150*2+25*5=705
                "food" => ("營養食物包", 605m),    // 45*3+35*4+25*6+15*12=605
                "hygiene" => ("清潔護理包", 415m), // 85*1+75*1+20*4+35*5=415
                _ => ("", 0m)
            };

            // 驗證組合包類型
            if (price == 0)
            {
                TempData["Error"] = "找不到指定的組合包";
                return RedirectToAction("Index");
            }

            // 驗證數量範圍
            if (quantity < 1 || quantity > 3)
            {
                TempData["Error"] = "數量必須在1-3份之間";
                return RedirectToAction("Index");
            }

            // 建立組合包付款資料模型
            var paymentModel = new PaymentViewModel
            {
                SupplyId = -1, // 組合包標記（-1表示非單項物資）
                SupplyName = name,
                Quantity = quantity,
                TotalPrice = price * quantity,
                SupplyType = "package",
                IsLoggedIn = User.Identity?.IsAuthenticated ?? false
            };

            // 預填已登入用戶資訊
            if (paymentModel.IsLoggedIn)
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        paymentModel.UserId = userId;
                        paymentModel.DonorName = user.Name ?? "";
                        paymentModel.DonorEmail = user.Email ?? "";
                        paymentModel.DonorPhone = user.Phone ?? "";
                    }
                }
            }

            // 暫存組合包類型供後續處理使用
            TempData["PackageType"] = packageType;
            return View("Payment", paymentModel);
        }


        // 處理付款 - 模擬付款流程並更新資料庫
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            // 驗證表單資料
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            try
            {
                // 驗證用戶是否存在（如果已登入）
                if (model.IsLoggedIn && model.UserId.HasValue)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                    if (!userExists)
                    {
                        ModelState.AddModelError("", $"用戶ID {model.UserId} 在資料庫中不存在");
                        return View("Payment", model);
                    }
                }
                
                // 產生訂單編號
                var orderNumber = GenerateOrderNumber();
                
                // 檢查是否為組合包
                bool isPackage = model.SupplyType == "package";
                
                // 宣告訂單變數
                UserOrder? order = null;
                
                if (isPackage)
                {
                    // 處理組合包購買 - 需要記錄所有包含的物資
                    var packageType = TempData["PackageType"]?.ToString();
                    
                    // 檢查packageType是否有效
                    if (string.IsNullOrEmpty(packageType))
                    {
                        ModelState.AddModelError("", "組合包類型資訊遺失，請重新選擇");
                        return View("Payment", model);
                    }
                    
                    // 定義組合包包含的物資（每個組合包包含多種物資）
                    var packageItems = packageType switch
                    {
                        "medical" => new List<(int supplyId, int quantity)> 
                        { 
                            (14, 2), // 醫療口罩 x 2盒 (80x2=160)
                            (15, 2), // 酒精消毒液 x 2瓶 (60x2=120)
                            (18, 2), // 體溫計 x 2支 (150x2=300)
                            (19, 5)  // 紗布繃帶 x 5組 (25x5=125) 總計:705元
                        },
                        "food" => new List<(int supplyId, int quantity)> 
                        { 
                            (3, 3),  // 糙米 x 3包 (45x3=135)
                            (4, 4),  // 玉米罐頭 x 4罐 (35x4=140)
                            (5, 6),  // 蘋果麵包 x 6個 (25x6=150)
                            (7, 12)  // 豆奶 x 12瓶 (15x12=180) 總計:605元
                        },
                        "hygiene" => new List<(int supplyId, int quantity)> 
                        { 
                            (25, 1), // 洗髮精 x 1瓶 (85x1=85)
                            (26, 1), // 沐浴乳 x 1瓶 (75x1=75)
                            (27, 4), // 肥皂 x 4塊 (20x4=80)
                            (28, 5)  // 濕紙巾 x 5包 (35x5=175) 總計:415元
                        },
                        _ => new List<(int supplyId, int quantity)>()
                    };
                    
                    // 確保packageItems不為空
                    if (!packageItems.Any())
                    {
                        ModelState.AddModelError("", $"無效的組合包類型: {packageType}");
                        return View("Payment", model);
                    }

                    // 如果是已登入用戶，創建訂單記錄
                    if (model.IsLoggedIn && model.UserId.HasValue)
                    {
                        order = new UserOrder
                        {
                            UserId = model.UserId.Value,
                            OrderNumber = orderNumber,
                            OrderDate = DateTime.Now,
                            TotalPrice = model.TotalPrice,
                            PaymentStatus = model.PaymentMethod == "ecpay" ? "待付款" : "已付款",
                            PaymentMethod = model.PaymentMethod,
                            OrderSource = "package"
                        };
                        _context.UserOrders.Add(order);
                        await _context.SaveChangesAsync(); // 先保存訂單以取得OrderId
                        
                        // 為組合包中的每個物資創建訂單明細並增加庫存
                        var successCount = 0;
                        var missingSupplies = new List<int>();
                        
                        foreach (var (supplyId, quantity) in packageItems)
                        {
                            var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                            if (supply != null)
                            {
                                // 只有非ECPay付款才立即更新庫存，ECPay等回調確認後再更新
                                if (model.PaymentMethod != "ecpay")
                                {
                                    // 增加物資庫存（民眾捐款讓NGO採購物資）
                                    // 庫存更新 = 基本數量 x 組合包數量
                                    supply.SupplyQuantity += quantity * model.Quantity;
                                }
                                
                                // 創建訂單明細（使用實際物資價格）
                                // 注意：quantity 是基本數量，要乘以用戶選擇的組合包數量
                                var orderDetail = new UserOrderDetail
                                {
                                    UserOrderId = order.UserOrderId,
                                    SupplyId = supplyId,
                                    Quantity = quantity * model.Quantity, // 基本數量 x 組合包數量
                                    UnitPrice = supply.SupplyPrice ?? 0, // 使用實際物資價格
                                    OrderSource = "package"
                                };
                                _context.UserOrderDetails.Add(orderDetail);
                                successCount++;
                            }
                            else
                            {
                                missingSupplies.Add(supplyId);
                            }
                        }
                        
                        // 詳細的調試信息
                        var debugInfo = $"組合包認購 ({packageType}): 成功處理 {successCount}/{packageItems.Count} 項物資";
                        if (missingSupplies.Any())
                        {
                            debugInfo += $", 缺少物資ID: [{string.Join(",", missingSupplies)}]";
                        }
                    }
                    else
                    {
                        // 未登入用戶也要增加庫存（匿名捐贈）
                        foreach (var (supplyId, quantity) in packageItems)
                        {
                            var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                            if (supply != null && model.PaymentMethod != "ecpay")
                            {
                                // 庫存更新 = 基本數量 x 組合包數量
                                supply.SupplyQuantity += quantity * model.Quantity;
                            }
                        }
                    }
                }
                else
                {
                    // 如果是緊急需求，只處理緊急物資表，不涉及物資總表
                    if (model.EmergencyNeedId.HasValue)
                    {
                        // 只有非ECPay付款才立即更新庫存，ECPay等回調確認後再更新
                        if (model.PaymentMethod != "ecpay")
                        {
                            // 使用原始 SQL 更新以避免觸發器衝突
                            var emergencyNeedId = model.EmergencyNeedId.Value;
                            var quantity = model.Quantity;
                            
                            await _context.Database.ExecuteSqlRawAsync(
                                "UPDATE EmergencySupplyNeeds SET CollectedQuantity = CollectedQuantity + {0}, UpdatedDate = {1} WHERE EmergencyNeedId = {2}",
                                quantity, DateTime.Now, emergencyNeedId);
                            
                            // 檢查是否需要更新狀態
                            var emergencyNeed = await _context.EmergencySupplyNeeds
                                .FirstOrDefaultAsync(e => e.EmergencyNeedId == emergencyNeedId);
                            
                            if (emergencyNeed != null && emergencyNeed.CollectedQuantity >= emergencyNeed.Quantity && emergencyNeed.Status == "Fundraising")
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "UPDATE EmergencySupplyNeeds SET Status = 'Completed' WHERE EmergencyNeedId = {0}",
                                    emergencyNeedId);
                            }
                        }
                    }
                    else
                    {
                        // 處理一般物資認購（只有常規物資需要 SupplyId）
                        if (model.SupplyId.HasValue)
                        {
                            var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == model.SupplyId.Value);
                            if (supply == null)
                            {
                                ModelState.AddModelError("", "找不到指定的物資項目");
                                return View("Payment", model);
                            }

                            // 只有非ECPay付款才立即更新庫存，ECPay等回調確認後再更新
                            if (model.PaymentMethod != "ecpay")
                            {
                                // 認購物資：增加物資庫存（民眾捐錢讓NGO採購物資）
                                supply.SupplyQuantity += model.Quantity;
                            }
                        }
                    }

                    // 如果已登入用戶，記錄訂單
                    if (model.IsLoggedIn && model.UserId.HasValue)
                    {
                        try
                        {
                            // 先檢查用戶是否存在
                            var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                            if (!userExists)
                            {
                                // 用戶不存在時不處理
                            }
                            else
                            {
                                // 創建單項物資訂單
                                order = new UserOrder
                                {
                                    UserId = model.UserId.Value,
                                    OrderNumber = orderNumber,
                                    OrderDate = DateTime.Now,
                                    TotalPrice = model.TotalPrice,
                                    PaymentStatus = model.PaymentMethod == "ecpay" ? "待付款" : "已付款",
                                    PaymentMethod = model.PaymentMethod,
                                    OrderSource = model.EmergencyNeedId.HasValue ? "emergency" : "regular",
                                    EmergencyNeedId = model.EmergencyNeedId
                                };

                                _context.UserOrders.Add(order);
                                await _context.SaveChangesAsync();

                                // 根據訂單類型創建不同的訂單明細
                                if (model.EmergencyNeedId.HasValue)
                                {
                                    // 緊急物資認購 - 創建緊急物資認購記錄
                                    var emergencyRecord = new EmergencyPurchaseRecord
                                    {
                                        UserOrderId = order.UserOrderId,
                                        EmergencyNeedId = model.EmergencyNeedId.Value,
                                        SupplyName = model.SupplyName,
                                        Quantity = model.Quantity,
                                        UnitPrice = model.TotalPrice / model.Quantity,
                                        PurchaseDate = DateTime.Now,
                                        CaseId = model.CaseId ?? 0,
                                        PaymentMethod = model.PaymentMethod
                                    };
                                    _context.EmergencyPurchaseRecords.Add(emergencyRecord);
                                }
                                else
                                {
                                    // 一般物資認購 - 創建訂單明細（只有常規物資需要 SupplyId）
                                    if (model.SupplyId.HasValue)
                                    {
                                        var orderDetail = new UserOrderDetail
                                        {
                                            UserOrderId = order.UserOrderId,
                                            SupplyId = model.SupplyId.Value,
                                            Quantity = model.Quantity,
                                            UnitPrice = model.TotalPrice / model.Quantity,
                                            OrderSource = "regular",
                                            EmergencyNeedId = null
                                        };
                                        _context.UserOrderDetails.Add(orderDetail);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // 不拋出異常，讓付款流程繼續
                        }
                    }
                    else
                    {
                        // 未登入用戶的匿名捐贈，已在上面處理完成
                    }
                }

                // 保存所有變更
                await _context.SaveChangesAsync();

                // 建立成功結果視圖模型
                var result = new OrderResultViewModel
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    SupplyName = model.SupplyName,
                    Quantity = model.Quantity,
                    TotalPrice = model.TotalPrice,
                    DonorName = model.DonorName ?? "匿名捐贈者",
                    PaymentStatus = "付款成功",
                    PaymentMethod = model.PaymentMethod,
                    IsEmergency = model.SupplyType == "emergency",
                    CaseId = model.CaseId
                };

                // 如果選擇 ECPay，則跳轉到 ECPay 付款
                if (model.PaymentMethod == "ecpay" && order != null)
                {
                    // 產生 ECPay 付款表單
                    var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                    var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", new { MerchantTradeNo = order.OrderNumber }, Request.Scheme);
                    
                    // 調試信息
                    ViewBag.DebugReturnUrl = returnUrl;
                    ViewBag.DebugClientBackUrl = clientBackUrl;
                    ViewBag.DebugOrderNumber = order.OrderNumber;
                    ViewBag.UseNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                    ViewBag.NgrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");
                    
                    var paymentForm = _ecpayService.CreatePayment(order, returnUrl!, clientBackUrl!);

                    ViewBag.PaymentForm = paymentForm;
                    return View("EcpayRedirect");
                }
                
                return View("Success", result);
            }
            catch (Exception ex)
            {
                // 獲取完整錯誤信息包括inner exception
                var fullError = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    fullError += " Inner: " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                }
                
                ModelState.AddModelError("", "處理付款時發生錯誤: " + fullError);
                return View("Payment", model);
            }
        }

        // 取得組合包物資列表 (調整數量以符合實際價格)
        private List<(int supplyId, int quantity)> GetPackageItems(string packageType)
        {
            return packageType switch
            {
                "medical" => new List<(int, int)>
                {
                    (14, 2), // 醫療口罩 x 2盒 (80x2=160)
                    (15, 2), // 酒精消毒液 x 2瓶 (60x2=120)
                    (18, 2), // 體溫計 x 2支 (150x2=300)
                    (19, 5)  // 紗布繃帶 x 5組 (25x5=125) 總計:705元
                },
                "food" => new List<(int, int)>
                {
                    (3, 3),  // 糙米 x 3包 (45x3=135)
                    (4, 4),  // 玉米罐頭 x 4罐 (35x4=140)
                    (5, 6),  // 蘋果麵包 x 6個 (25x6=150)
                    (7, 12)  // 豆奶 x 12瓶 (15x12=180) 總計:605元
                },
                "hygiene" => new List<(int, int)>
                {
                    (25, 1), // 洗髮精 x 1瓶 (85x1=85)
                    (26, 1), // 沐浴乳 x 1瓶 (75x1=75)
                    (27, 4), // 肥皂 x 4塊 (20x4=80)
                    (28, 5)  // 濕紙巾 x 5包 (35x5=175) 總計:415元
                },
                _ => new List<(int, int)>()
            };
        }

        // 取得組合包基本資訊 (根據實際物資價格)
        private (string name, decimal price)? GetPackageInfo(string packageType)
        {
            return packageType switch
            {
                "medical" => ("醫療照護包", 705m),  // 80*2+60*2+150*2+25*5=705
                "food" => ("營養食物包", 605m),     // 45*3+35*4+25*6+15*12=605
                "hygiene" => ("清潔護理包", 415m),  // 85*1+75*1+20*4+35*5=415
                _ => null
            };
        }

        // 產生訂單編號: NGO{yyyyMMddHHmmss} 例如 NGO20250727143052
        // 使用時間戳記確保絕對唯一性，符合業界標準
        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"NGO{timestamp}";
        }

        // 智慧型預設圖片選擇 - 根據物資名稱自動匹配圖片
        private static string GetDefaultEmergencyImage(string supplyName)
        {
            var name = supplyName?.ToLower() ?? "";
            
            if (name.Contains("尿布") || name.Contains("紙尿褲"))
                return "https://images.unsplash.com/photo-1584462256711-aa4c7c8e1949?w=400&h=250&fit=crop";
            else if (name.Contains("口罩") || name.Contains("醫療"))
                return "https://images.unsplash.com/photo-1584931423298-c576fda54bd2?w=400&h=250&fit=crop";
            else if (name.Contains("食物") || name.Contains("米") || name.Contains("麵"))
                return "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400&h=250&fit=crop";
            else if (name.Contains("藥") || name.Contains("醫"))
                return "https://images.unsplash.com/photo-1559757148-5c350d0d3c56?w=400&h=250&fit=crop";
            else if (name.Contains("衣") || name.Contains("毛巾"))
                return "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=400&h=250&fit=crop";
            else if (name.Contains("牛奶") || name.Contains("奶粉"))
                return "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=400&h=250&fit=crop";
            else
                return "https://images.unsplash.com/photo-1559757175-0eb30cd8c063?w=400&h=250&fit=crop";
        }

        // ECPay 付款處理
        [HttpPost]
        public IActionResult EcpayPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            try
            {
                // 建立訂單
                var order = new UserOrder
                {
                    UserId = model.UserId ?? 0,
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = DateTime.Now,
                    TotalPrice = model.TotalPrice,
                    PaymentStatus = "待付款",
                    PaymentMethod = "ecpay",
                    OrderSource = model.SupplyType ?? "regular",
                    EmergencyNeedId = model.EmergencyNeedId
                };

                _context.UserOrders.Add(order);
                _context.SaveChanges();

                // 建立訂單明細
                if (model.SupplyId.HasValue)
                {
                    var orderDetail = new UserOrderDetail
                    {
                        UserOrderId = order.UserOrderId,
                        SupplyId = model.SupplyId.Value,
                        Quantity = model.Quantity,
                        UnitPrice = model.TotalPrice / model.Quantity,
                        EmergencyNeedId = model.EmergencyNeedId
                    };
                    _context.UserOrderDetails.Add(orderDetail);
                    _context.SaveChanges();
                }

                // 產生 ECPay 付款表單
                var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", null, Request.Scheme);
                
                // 調試信息
                ViewBag.DebugReturnUrl = returnUrl;
                ViewBag.DebugClientBackUrl = clientBackUrl;
                ViewBag.DebugOrderNumber = order.OrderNumber;
                ViewBag.UseNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                ViewBag.NgrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");
                
                var paymentForm = _ecpayService.CreatePayment(order, returnUrl!, clientBackUrl!);

                ViewBag.PaymentForm = paymentForm;
                return View("EcpayRedirect");
            }
            catch (Exception ex)
            {
                var errorMessage = "建立付款時發生錯誤: " + ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " 內部錯誤: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += " 更詳細錯誤: " + ex.InnerException.InnerException.Message;
                    }
                }
                ModelState.AddModelError("", errorMessage);
                return View("Payment", model);
            }
        }

        // ECPay 付款回調
        [HttpPost]
        [AllowAnonymous]
        public IActionResult EcpayCallback()
        {
            var parameters = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            
            // 記錄回調參數供調試
            var merchantTradeNo = parameters.GetValueOrDefault("MerchantTradeNo");
            var rtnCode = parameters.GetValueOrDefault("RtnCode");
            var tradeNo = parameters.GetValueOrDefault("TradeNo");
            
            Console.WriteLine($"=== ECPay 回調 ===");
            Console.WriteLine($"MerchantTradeNo: {merchantTradeNo}");
            Console.WriteLine($"RtnCode: {rtnCode}");
            Console.WriteLine($"TradeNo: {tradeNo}");
            Console.WriteLine($"所有回調參數:");
            foreach (var param in parameters)
            {
                Console.WriteLine($"  {param.Key} = {param.Value}");
            }
            
            var success = _ecpayService.ProcessCallback(parameters);
            
            Console.WriteLine($"回調處理結果: {(success ? "成功" : "失敗")}");
            
            return Content(success ? "1|OK" : "0|ERROR");
        }

        // 付款成功頁面
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess()
        {
            // 嘗試從多個來源獲取訂單編號
            string? merchantTradeNo = Request.Query["MerchantTradeNo"].FirstOrDefault() 
                                    ?? Request.Form["MerchantTradeNo"].FirstOrDefault()
                                    ?? Request.Query["orderNumber"].FirstOrDefault();

            if (string.IsNullOrEmpty(merchantTradeNo))
            {
                // 如果沒有訂單編號，顯示通用成功頁面
                var genericResult = new OrderResultViewModel
                {
                    OrderNumber = "未知",
                    OrderDate = DateTime.Now,
                    TotalPrice = 0,
                    PaymentStatus = "付款成功",
                    PaymentMethod = "ECPay",
                    DonorName = "匿名捐贈者"
                };
                return View("Success", genericResult);
            }

            var order = _context.UserOrders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderNumber == merchantTradeNo);

            if (order == null)
            {
                // 即使找不到訂單，也顯示成功頁面
                var fallbackResult = new OrderResultViewModel
                {
                    OrderNumber = merchantTradeNo,
                    OrderDate = DateTime.Now,
                    TotalPrice = 0,
                    PaymentStatus = "付款成功",
                    PaymentMethod = "ECPay",
                    DonorName = "匿名捐贈者"
                };
                return View("Success", fallbackResult);
            }

            // 獲取用戶名稱
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == order.UserId);
            var donorName = user?.Name ?? "匿名捐贈者";
            
            // 取得訂單詳情 - 修復數量顯示問題
            var supplyName = "物資捐贈";
            var quantity = 0;
            var isEmergency = false;
            int? caseId = null;

            if (order.OrderDetails.Any())
            {
                // 檢查是否為組合包訂單（通過訂單來源判斷）
                var isPackageOrder = order.OrderSource == "package";
                
                if (isPackageOrder)
                {
                    // 組合包：需要從 OrderDetail 計算出組合包數量
                    // 因為每個 OrderDetail 的 Quantity = 基本數量 x 組合包數量
                    // 我們取第一個物資的數量來反推組合包數量
                    var firstDetail = order.OrderDetails.FirstOrDefault();
                    if (firstDetail != null)
                    {
                        // 根據組合包類型確定基本倍數來計算組合包數量
                        // 醫療包第一項是口罩2盒，食物包第一項是糙米3包，清潔包第一項是洗髮精1瓶
                        var baseQuantity = firstDetail.SupplyId switch
                        {
                            14 => 2, // 醫療包 - 口罩基本2盒
                            3 => 3,  // 食物包 - 糙米基本3包  
                            16 => 1, // 清潔包 - 洗髮精基本1瓶
                            _ => 1
                        };
                        quantity = firstDetail.Quantity / baseQuantity;
                        
                        // 組合包名稱處理
                        var supply = await _context.Supplies
                            .FirstOrDefaultAsync(s => s.SupplyId == firstDetail.SupplyId);
                        
                        // 根據第一個物資判斷組合包類型
                        supplyName = firstDetail.SupplyId switch
                        {
                            14 => "醫療照護套組", // 口罩
                            3 => "營養食物套組",  // 糙米
                            16 => "清潔護理套組", // 洗髮精
                            _ => "愛心組合包"
                        };
                    }
                    else
                    {
                        quantity = 1;
                        supplyName = "愛心組合包";
                    }
                }
                else if (order.OrderDetails.Count > 1)
                {
                    // 多個物資的情況（非組合包）
                    quantity = order.OrderDetails.Sum(od => od.Quantity);
                    var supplyNames = new List<string>();
                    
                    foreach (var detail in order.OrderDetails)
                    {
                        if (detail.EmergencyNeedId.HasValue)
                        {
                            var emergency = await _context.EmergencySupplyNeeds
                                .FirstOrDefaultAsync(e => e.EmergencyNeedId == detail.EmergencyNeedId.Value);
                            if (emergency != null)
                            {
                                supplyNames.Add($"{emergency.SupplyName}({detail.Quantity}份)");
                                isEmergency = true;
                                caseId = emergency.CaseId;
                            }
                        }
                        else
                        {
                            var supply = await _context.Supplies
                                .FirstOrDefaultAsync(s => s.SupplyId == detail.SupplyId);
                            if (supply != null)
                            {
                                supplyNames.Add($"{supply.SupplyName}({detail.Quantity}份)");
                            }
                        }
                    }
                    
                    supplyName = string.Join("、", supplyNames);
                    if (supplyNames.Count > 2)
                    {
                        supplyName = $"多項物資 (共{quantity}份)";
                    }
                }
                else
                {
                    // 單一物資
                    var orderDetail = order.OrderDetails.First();
                    quantity = orderDetail.Quantity;
                    
                    if (orderDetail.EmergencyNeedId.HasValue)
                    {
                        // 緊急物資
                        var emergency = await _context.EmergencySupplyNeeds
                            .FirstOrDefaultAsync(e => e.EmergencyNeedId == orderDetail.EmergencyNeedId.Value);
                        if (emergency != null)
                        {
                            supplyName = emergency.SupplyName ?? "緊急物資";
                            isEmergency = true;
                            caseId = emergency.CaseId;
                        }
                    }
                    else
                    {
                        // 一般物資
                        var supply = await _context.Supplies
                            .FirstOrDefaultAsync(s => s.SupplyId == orderDetail.SupplyId);
                        if (supply != null)
                        {
                            supplyName = supply.SupplyName ?? "一般物資";
                        }
                    }
                }
            }
            
            // 如果是緊急訂單，從 EmergencyPurchaseRecords 取得資訊和數量
            if (order.EmergencyNeedId.HasValue && !isEmergency)
            {
                var emergencyRecord = await _context.EmergencyPurchaseRecords
                    .FirstOrDefaultAsync(e => e.UserOrderId == order.UserOrderId);
                    
                if (emergencyRecord != null)
                {
                    supplyName = emergencyRecord.SupplyName ?? "緊急物資";
                    quantity = emergencyRecord.Quantity; // 從緊急認購記錄取得正確數量
                    isEmergency = true;
                    caseId = emergencyRecord.CaseId;
                }
                else
                {
                    // 如果找不到緊急認購記錄，從需求表取得基本資訊
                    var emergency = await _context.EmergencySupplyNeeds
                        .FirstOrDefaultAsync(e => e.EmergencyNeedId == order.EmergencyNeedId.Value);
                    if (emergency != null)
                    {
                        supplyName = emergency.SupplyName ?? "緊急物資";
                        quantity = 1; // 預設數量
                        isEmergency = true;
                        caseId = emergency.CaseId;
                    }
                }
            }

            var viewModel = new OrderResultViewModel
            {
                OrderNumber = order.OrderNumber!,
                OrderDate = order.OrderDate,
                SupplyName = supplyName,
                Quantity = quantity,
                TotalPrice = order.TotalPrice,
                PaymentStatus = order.PaymentStatus!,
                PaymentMethod = "ECPay",
                DonorName = donorName,
                IsEmergency = isEmergency,
                CaseId = caseId
            };

            return View("Success", viewModel);
        }

        // 重新付款功能
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            // 調試信息
            Console.WriteLine($"RetryPayment 被調用，OrderId: {orderId}");
            
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"UserIdStr: {userIdStr}");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                Console.WriteLine("用戶身份驗證失敗");
                TempData["Error"] = "用戶身份驗證失敗";
                return RedirectToAction("Login", "Auth");
            }
            
            Console.WriteLine($"UserId: {userId}");

            // 查找用戶的未付款訂單
            var order = await _context.UserOrders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Supply)
                .FirstOrDefaultAsync(o => o.UserOrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = $"找不到指定的訂單 (OrderId: {orderId}, UserId: {userId})";
                return RedirectToAction("PurchaseRecords", "User");
            }

            // 只允許重新付款未付款或失敗的訂單
            if (order.PaymentStatus != "待付款" && order.PaymentStatus != "付款失敗")
            {
                TempData["Error"] = "此訂單無法重新付款";
                return RedirectToAction("PurchaseRecords", "User");
            }

            // 檢查訂單是否太舊（比如超過24小時）
            if ((DateTime.Now - order.OrderDate).TotalHours > 24)
            {
                TempData["Error"] = "訂單已過期，請重新下單";
                return RedirectToAction("PurchaseRecords", "User");
            }

            try
            {
                // 為重新付款生成新的訂單號（ECPay 限制20字元）
                var timestamp = DateTime.Now.ToString("mmss"); // 4位數字，更短
                var baseOrderNumber = order.OrderNumber;
                
                // 如果已經是重新付款的訂單號，替換時間戳部分
                if (baseOrderNumber.Contains("R") && baseOrderNumber.Length > 18)
                {
                    baseOrderNumber = baseOrderNumber.Substring(0, baseOrderNumber.LastIndexOf("R"));
                }
                
                // 確保總長度不超過20字元：基礎號(15) + R(1) + 時間戳(4) = 20
                if (baseOrderNumber.Length > 15)
                {
                    baseOrderNumber = baseOrderNumber.Substring(0, 15);
                }
                
                var newOrderNumber = $"{baseOrderNumber}R{timestamp}";
                order.OrderNumber = newOrderNumber;
                order.PaymentStatus = "待付款";
                await _context.SaveChangesAsync();

                // 產生 ECPay 付款表單
                var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", new { MerchantTradeNo = order.OrderNumber }, Request.Scheme);
                
                // 調試信息
                ViewBag.DebugReturnUrl = returnUrl;
                ViewBag.DebugClientBackUrl = clientBackUrl;
                ViewBag.DebugOrderNumber = order.OrderNumber;
                ViewBag.UseNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                ViewBag.NgrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");
                
                var paymentForm = _ecpayService.CreatePayment(order, returnUrl!, clientBackUrl!);

                ViewBag.PaymentForm = paymentForm;
                return View("EcpayRedirect");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "重新付款時發生錯誤: " + ex.Message;
                return RedirectToAction("PurchaseRecords", "User");
            }
        }

        /// <summary>
        /// 根據緊急物資名稱取得合理價格（基於網路市價）
        /// </summary>
        private decimal GetEmergencySupplyPrice(string supplyName)
        {
            return supplyName?.ToLower() switch
            {
                // 醫療相關
                var name when name.Contains("血糖機") || name.Contains("血糖檢測") => 800m,
                var name when name.Contains("醫療急救包") || name.Contains("急救包") => 350m,
                var name when name.Contains("體溫計") || name.Contains("溫度計") => 150m,
                var name when name.Contains("血壓計") => 1200m,
                
                // 嬰幼兒用品
                var name when name.Contains("尿布") && (name.Contains("小孩") || name.Contains("嬰兒") || name.Contains("幼兒")) => 12m, // 每片
                var name when name.Contains("尿布") && name.Contains("成人") => 15m, // 每片成人尿布
                var name when name.Contains("紙尿褲") && name.Contains("成人") => 18m, // 每片成人紙尿褲
                var name when name.Contains("紙尿褲") && (name.Contains("小孩") || name.Contains("嬰兒")) => 10m, // 每片嬰兒紙尿褲
                var name when name.Contains("奶粉") => 450m,
                var name when name.Contains("副食品") || name.Contains("嬰兒食品") => 80m,
                
                // 保暖用品
                var name when name.Contains("棉被") && name.Contains("冬季") => 800m,
                var name when name.Contains("棉被") => 600m,
                var name when name.Contains("毛衣") || name.Contains("保暖衣") => 350m,
                var name when name.Contains("毛毯") => 400m,
                var name when name.Contains("睡袋") => 1200m,
                
                // 食物類
                var name when name.Contains("即食") && name.Contains("罐頭") => 35m,
                var name when name.Contains("罐頭") => 30m,
                var name when name.Contains("泡麵") || name.Contains("速食麵") => 25m,
                var name when name.Contains("米") && name.Contains("包") => 150m, // 一包米
                var name when name.Contains("食用油") => 120m,
                var name when name.Contains("麵條") || name.Contains("麵食") => 80m,
                
                // 家電用品
                var name when name.Contains("微波爐") => 2500m,
                var name when name.Contains("電熱水壺") => 800m,
                var name when name.Contains("電風扇") => 1200m,
                var name when name.Contains("電暖器") => 1500m,
                
                // 清潔用品
                var name when name.Contains("洗髮精") => 120m,
                var name when name.Contains("沐浴乳") => 100m,
                var name when name.Contains("洗衣精") => 150m,
                var name when name.Contains("衛生紙") => 80m,
                var name when name.Contains("濕紙巾") => 60m,
                
                // 學習用品
                var name when name.Contains("筆記本") || name.Contains("作業本") => 30m,
                var name when name.Contains("文具") => 50m,
                var name when name.Contains("書包") => 300m,
                
                // 預設價格
                _ => 100m
            };
        }
    }
}
