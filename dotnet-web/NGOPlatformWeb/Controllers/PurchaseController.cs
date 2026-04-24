using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.ViewModels.Purchase;
using NGOPlatformWeb.Services;

namespace NGOPlatformWeb.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        private readonly EcpayService _ecpayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(
            IPurchaseService purchaseService,
            EcpayService ecpayService,
            IConfiguration configuration,
            ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _ecpayService = ecpayService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var emergencyNeeds = new List<object>();
                var regularSupplies = new List<object>();

                try
                {
                    var rawEmergencyNeeds = await _purchaseService.GetActiveEmergencyNeedsAsync();
                    emergencyNeeds = rawEmergencyNeeds.Select(e =>
                    {
                        dynamic obj = new System.Dynamic.ExpandoObject();
                        obj.Id = e.EmergencyNeedId;
                        obj.CaseId = e.CaseId;
                        obj.SupplyName = e.SupplyName ?? "未知物資";
                        obj.NeededQuantity = e.Quantity;
                        obj.RemainingQuantity = Math.Max(0, e.Quantity - e.CollectedQuantity);
                        obj.ImageUrl = !string.IsNullOrEmpty(e.ImageUrl) ? e.ImageUrl : GetDefaultEmergencyImage(e.SupplyName ?? "");
                        obj.Status = e.Status ?? "未知狀態";
                        obj.Priority = e.Priority ?? "Normal";
                        obj.Description = e.Description ?? "無描述";
                        return (object)obj;
                    }).ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.EmergencyError = "緊急需求資料載入失敗: " + ex.Message;
                }

                try
                {
                    var rawRegularSupplies = await _purchaseService.GetRegularSuppliesAsync();
                    regularSupplies = rawRegularSupplies.Cast<object>().ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.RegularError = "常規物資資料載入失敗: " + ex.Message;
                }

                ViewBag.EmergencyNeeds = emergencyNeeds;
                ViewBag.RegularSupplies = regularSupplies;

                return View();
            }
            catch (Exception)
            {
                ViewBag.EmergencyNeeds = new List<object>();
                ViewBag.RegularSupplies = new List<object>();
                ViewBag.Error = "頁面載入發生錯誤，請稍後再試";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectPurchase(int? supplyId = null, int quantity = 1, int? emergencyNeedId = null)
        {
            try
            {
                if (emergencyNeedId.HasValue)
                {
                    var emergencyNeed = await _purchaseService.GetEmergencyNeedByIdAsync(emergencyNeedId.Value);
                    if (emergencyNeed == null)
                    {
                        TempData["Error"] = "找不到指定的緊急需求項目";
                        return RedirectToAction("Index");
                    }

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

                    string? caseDescription = null;
                    if (emergencyNeed.CaseId > 0)
                    {
                        var caseEntity = await _purchaseService.GetCaseByIdAsync(emergencyNeed.CaseId);
                        caseDescription = caseEntity?.Description;
                    }

                    decimal unitPrice = GetEmergencySupplyPrice(emergencyNeed.SupplyName ?? "");

                    var paymentModel = new PaymentViewModel
                    {
                        SupplyName = emergencyNeed.SupplyName ?? "未知物資",
                        Quantity = quantity,
                        TotalPrice = unitPrice * quantity,
                        SupplyType = "emergency",
                        EmergencyNeedId = emergencyNeedId,
                        CaseId = emergencyNeed.CaseId,
                        CaseDescription = caseDescription,
                        MaxQuantity = remainingQuantity,
                        IsLoggedIn = User.Identity?.IsAuthenticated ?? false
                    };

                    if (paymentModel.IsLoggedIn)
                    {
                        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                        if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                        {
                            var user = await _purchaseService.GetUserByIdAsync(userId);
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

                if (!supplyId.HasValue)
                {
                    TempData["Error"] = "缺少物資項目資訊";
                    return RedirectToAction("Index");
                }

                var supply = await _purchaseService.GetSupplyByIdAsync(supplyId.Value);
                if (supply == null)
                {
                    TempData["Error"] = "找不到指定的物資項目";
                    return RedirectToAction("Index");
                }

                if (supply.SupplyQuantity < quantity)
                {
                    TempData["Error"] = $"庫存不足，目前只剩 {supply.SupplyQuantity} 份";
                    return RedirectToAction("Index");
                }

                var regularPaymentModel = new PaymentViewModel
                {
                    SupplyId = supply.SupplyId,
                    SupplyName = supply.SupplyName ?? "未知物資",
                    Quantity = quantity,
                    TotalPrice = (supply.SupplyPrice ?? 0) * quantity,
                    SupplyType = supply.SupplyType,
                    IsLoggedIn = User.Identity?.IsAuthenticated ?? false
                };

                if (regularPaymentModel.IsLoggedIn)
                {
                    var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                    if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                    {
                        var user = await _purchaseService.GetUserByIdAsync(userId);
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

        public IActionResult Payment()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyPackage(string packageType, int quantity = 1)
        {
            var (name, price) = packageType switch
            {
                "medical" => ("醫療照護包", 705m),
                "food" => ("營養食物包", 605m),
                "hygiene" => ("清潔護理包", 415m),
                _ => ("", 0m)
            };

            if (price == 0)
            {
                TempData["Error"] = "找不到指定的組合包";
                return RedirectToAction("Index");
            }

            if (quantity < 1 || quantity > 3)
            {
                TempData["Error"] = "數量必須在1-3份之間";
                return RedirectToAction("Index");
            }

            var paymentModel = new PaymentViewModel
            {
                SupplyId = -1,
                SupplyName = name,
                Quantity = quantity,
                TotalPrice = price * quantity,
                SupplyType = "package",
                IsLoggedIn = User.Identity?.IsAuthenticated ?? false
            };

            if (paymentModel.IsLoggedIn)
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "User" && int.TryParse(userIdStr, out int userId))
                {
                    var user = await _purchaseService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        paymentModel.UserId = userId;
                        paymentModel.DonorName = user.Name ?? "";
                        paymentModel.DonorEmail = user.Email ?? "";
                        paymentModel.DonorPhone = user.Phone ?? "";
                    }
                }
            }

            TempData["PackageType"] = packageType;
            return View("Payment", paymentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Payment", model);

            try
            {
                var packageType = TempData["PackageType"]?.ToString();
                var result = await _purchaseService.ProcessPaymentAsync(model, packageType);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Error ?? "處理付款時發生錯誤");
                    return View("Payment", model);
                }

                var viewModel = new OrderResultViewModel
                {
                    OrderNumber = result.OrderNumber,
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

                if (model.PaymentMethod == "ecpay")
                {
                    if (result.Order == null)
                        return RedirectToAction("Login", "Auth");

                    var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                    var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", new { MerchantTradeNo = result.Order.OrderNumber }, Request.Scheme);

                    ViewBag.DebugReturnUrl = returnUrl;
                    ViewBag.DebugClientBackUrl = clientBackUrl;
                    ViewBag.DebugOrderNumber = result.Order.OrderNumber;
                    ViewBag.UseNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                    ViewBag.NgrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");

                    var paymentForm = _ecpayService.CreatePayment(result.Order, returnUrl!, clientBackUrl!);
                    ViewBag.PaymentForm = paymentForm;
                    return View("EcpayRedirect");
                }

                return View("Success", viewModel);
            }
            catch (Exception ex)
            {
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

        [HttpPost]
        public async Task<IActionResult> EcpayPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Payment", model);

            try
            {
                var packageType = model.SupplyType == "package" ? TempData["PackageType"]?.ToString() : null;
                var result = await _purchaseService.ProcessPaymentAsync(model, packageType);

                if (!result.Success || result.Order == null)
                {
                    ModelState.AddModelError("", result.Error ?? "建立付款時發生錯誤");
                    return View("Payment", model);
                }

                var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", null, Request.Scheme);

                ViewBag.DebugReturnUrl = returnUrl;
                ViewBag.DebugClientBackUrl = clientBackUrl;
                ViewBag.DebugOrderNumber = result.Order.OrderNumber;
                ViewBag.UseNgrok = _configuration.GetValue<bool>("ECPay:UseNgrok");
                ViewBag.NgrokUrl = _configuration.GetValue<string>("ECPay:NgrokUrl");

                var paymentForm = _ecpayService.CreatePayment(result.Order, returnUrl!, clientBackUrl!);
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
                        errorMessage += " 更詳細錯誤: " + ex.InnerException.InnerException.Message;
                }
                ModelState.AddModelError("", errorMessage);
                return View("Payment", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult EcpayCallback()
        {
            var parameters = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            var success = _ecpayService.ProcessCallback(parameters);
            return Content(success ? "1|OK" : "0|ERROR");
        }

        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess()
        {
            string? merchantTradeNo = Request.Query["MerchantTradeNo"].FirstOrDefault()
                                    ?? Request.Form["MerchantTradeNo"].FirstOrDefault()
                                    ?? Request.Query["orderNumber"].FirstOrDefault();

            if (string.IsNullOrEmpty(merchantTradeNo))
            {
                return View("Success", new OrderResultViewModel
                {
                    OrderNumber = "未知",
                    OrderDate = DateTime.Now,
                    TotalPrice = 0,
                    PaymentStatus = "付款成功",
                    PaymentMethod = "ECPay",
                    DonorName = "匿名捐贈者"
                });
            }

            var viewModel = await _purchaseService.GetEcpaySuccessViewModelAsync(merchantTradeNo);

            if (viewModel == null)
            {
                return View("Success", new OrderResultViewModel
                {
                    OrderNumber = merchantTradeNo,
                    OrderDate = DateTime.Now,
                    TotalPrice = 0,
                    PaymentStatus = "付款成功",
                    PaymentMethod = "ECPay",
                    DonorName = "匿名捐贈者"
                });
            }

            return View("Success", viewModel);
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            _logger.LogInformation("RetryPayment 被調用，OrderId: {OrderId}", orderId);

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                _logger.LogWarning("RetryPayment 用戶身份驗證失敗，OrderId: {OrderId}", orderId);
                TempData["Error"] = "用戶身份驗證失敗";
                return RedirectToAction("Login", "Auth");
            }

            var order = await _purchaseService.GetOrderByIdForUserAsync(orderId, userId);
            if (order == null)
            {
                TempData["Error"] = $"找不到指定的訂單 (OrderId: {orderId}, UserId: {userId})";
                return RedirectToAction("PurchaseRecords", "User");
            }

            if (order.PaymentStatus != "待付款" && order.PaymentStatus != "付款失敗")
            {
                TempData["Error"] = "此訂單無法重新付款";
                return RedirectToAction("PurchaseRecords", "User");
            }

            if ((DateTime.Now - order.OrderDate).TotalHours > 24)
            {
                TempData["Error"] = "訂單已過期，請重新下單";
                return RedirectToAction("PurchaseRecords", "User");
            }

            try
            {
                var timestamp = DateTime.Now.ToString("MMss");
                var baseOrderNumber = order.OrderNumber ?? "";

                if (baseOrderNumber.Contains("R") && baseOrderNumber.Length > 18)
                    baseOrderNumber = baseOrderNumber.Substring(0, baseOrderNumber.LastIndexOf("R"));

                if (baseOrderNumber.Length > 15)
                    baseOrderNumber = baseOrderNumber.Substring(0, 15);

                var newOrderNumber = $"{baseOrderNumber}R{timestamp}";
                await _purchaseService.UpdateOrderNumberAsync(order, newOrderNumber);

                var returnUrl = Url.Action("EcpayCallback", "Purchase", null, Request.Scheme);
                var clientBackUrl = Url.Action("PaymentSuccess", "Purchase", new { MerchantTradeNo = order.OrderNumber }, Request.Scheme);

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

        private static decimal GetEmergencySupplyPrice(string supplyName)
        {
            return supplyName.ToLower() switch
            {
                var name when name.Contains("血糖機") || name.Contains("血糖檢測") => 800m,
                var name when name.Contains("醫療急救包") || name.Contains("急救包") => 350m,
                var name when name.Contains("體溫計") || name.Contains("溫度計") => 150m,
                var name when name.Contains("血壓計") => 1200m,
                var name when name.Contains("尿布") && (name.Contains("小孩") || name.Contains("嬰兒") || name.Contains("幼兒")) => 12m,
                var name when name.Contains("尿布") && name.Contains("成人") => 15m,
                var name when name.Contains("紙尿褲") && name.Contains("成人") => 18m,
                var name when name.Contains("紙尿褲") && (name.Contains("小孩") || name.Contains("嬰兒")) => 10m,
                var name when name.Contains("奶粉") => 450m,
                var name when name.Contains("副食品") || name.Contains("嬰兒食品") => 80m,
                var name when name.Contains("棉被") && name.Contains("冬季") => 800m,
                var name when name.Contains("棉被") => 600m,
                var name when name.Contains("毛衣") || name.Contains("保暖衣") => 350m,
                var name when name.Contains("毛毯") => 400m,
                var name when name.Contains("睡袋") => 1200m,
                var name when name.Contains("即食") && name.Contains("罐頭") => 35m,
                var name when name.Contains("罐頭") => 30m,
                var name when name.Contains("泡麵") || name.Contains("速食麵") => 25m,
                var name when name.Contains("米") && name.Contains("包") => 150m,
                var name when name.Contains("食用油") => 120m,
                var name when name.Contains("麵條") || name.Contains("麵食") => 80m,
                var name when name.Contains("微波爐") => 2500m,
                var name when name.Contains("電熱水壺") => 800m,
                var name when name.Contains("電風扇") => 1200m,
                var name when name.Contains("電暖器") => 1500m,
                var name when name.Contains("洗髮精") => 120m,
                var name when name.Contains("沐浴乳") => 100m,
                var name when name.Contains("洗衣精") => 150m,
                var name when name.Contains("衛生紙") => 80m,
                var name when name.Contains("濕紙巾") => 60m,
                var name when name.Contains("筆記本") || name.Contains("作業本") => 30m,
                var name when name.Contains("文具") => 50m,
                var name when name.Contains("書包") => 300m,
                _ => 100m
            };
        }
    }
}
