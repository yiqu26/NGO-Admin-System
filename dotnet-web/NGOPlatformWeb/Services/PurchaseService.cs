using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Purchase;

namespace NGOPlatformWeb.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly NGODbContext _context;

        public PurchaseService(NGODbContext context)
        {
            _context = context;
        }

        public async Task<IList<EmergencySupplyNeeds>> GetActiveEmergencyNeedsAsync()
        {
            return await _context.EmergencySupplyNeeds
                .Where(e => e.Status == "Fundraising" && e.CollectedQuantity < e.Quantity)
                .OrderBy(e => e.Priority == "Urgent" ? 1 :
                              e.Priority == "High" ? 2 :
                              e.Priority == "Normal" ? 3 : 4)
                .ThenByDescending(e => (decimal)(e.Quantity - e.CollectedQuantity) / e.Quantity)
                .Take(6)
                .ToListAsync();
        }

        public async Task<IList<Supply>> GetRegularSuppliesAsync()
        {
            return await _context.Supplies
                .Include(s => s.SupplyCategory)
                .Where(s => s.SupplyType == "regular")
                .Take(30)
                .ToListAsync();
        }

        public async Task<EmergencySupplyNeeds?> GetEmergencyNeedByIdAsync(int id)
        {
            return await _context.EmergencySupplyNeeds.FirstOrDefaultAsync(e => e.EmergencyNeedId == id);
        }

        public async Task<Supply?> GetSupplyByIdAsync(int id)
        {
            return await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == id);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await Task.FromResult(_context.Users.FirstOrDefault(u => u.UserId == id));
        }

        public async Task<Case?> GetCaseByIdAsync(int id)
        {
            return await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == id);
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(PaymentViewModel model, string? packageType)
        {
            if (model.IsLoggedIn && model.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                if (!userExists)
                    return new ProcessPaymentResult { Error = $"用戶ID {model.UserId} 在資料庫中不存在" };
            }

            var orderNumber = GenerateOrderNumber();
            bool isPackage = model.SupplyType == "package";
            UserOrder? order = null;

            if (isPackage)
            {
                var packageItems = GetPackageItems(packageType ?? "");
                if (!packageItems.Any())
                    return new ProcessPaymentResult { Error = string.IsNullOrEmpty(packageType) ? "組合包類型資訊遺失，請重新選擇" : $"無效的組合包類型: {packageType}" };

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
                    await _context.SaveChangesAsync();

                    foreach (var (supplyId, quantity) in packageItems)
                    {
                        var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                        if (supply != null)
                        {
                            if (model.PaymentMethod != "ecpay")
                                supply.SupplyQuantity += quantity * model.Quantity;

                            _context.UserOrderDetails.Add(new UserOrderDetail
                            {
                                UserOrderId = order.UserOrderId,
                                SupplyId = supplyId,
                                Quantity = quantity * model.Quantity,
                                UnitPrice = supply.SupplyPrice ?? 0,
                                OrderSource = "package"
                            });
                        }
                    }
                }
                else
                {
                    foreach (var (supplyId, quantity) in packageItems)
                    {
                        var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == supplyId);
                        if (supply != null && model.PaymentMethod != "ecpay")
                            supply.SupplyQuantity += quantity * model.Quantity;
                    }
                }
            }
            else
            {
                if (model.EmergencyNeedId.HasValue)
                {
                    if (model.PaymentMethod != "ecpay")
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE EmergencySupplyNeeds SET CollectedQuantity = CollectedQuantity + {0}, UpdatedDate = {1} WHERE EmergencyNeedId = {2}",
                            model.Quantity, DateTime.Now, model.EmergencyNeedId.Value);

                        var emergencyNeed = await _context.EmergencySupplyNeeds
                            .FirstOrDefaultAsync(e => e.EmergencyNeedId == model.EmergencyNeedId.Value);

                        if (emergencyNeed != null &&
                            emergencyNeed.CollectedQuantity >= emergencyNeed.Quantity &&
                            emergencyNeed.Status == "Fundraising")
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                "UPDATE EmergencySupplyNeeds SET Status = 'Completed' WHERE EmergencyNeedId = {0}",
                                model.EmergencyNeedId.Value);
                        }
                    }
                }
                else if (model.SupplyId.HasValue)
                {
                    var supply = await _context.Supplies.FirstOrDefaultAsync(s => s.SupplyId == model.SupplyId.Value);
                    if (supply == null)
                        return new ProcessPaymentResult { Error = "找不到指定的物資項目" };

                    if (model.PaymentMethod != "ecpay")
                        supply.SupplyQuantity += model.Quantity;
                }

                if (model.IsLoggedIn && model.UserId.HasValue)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == model.UserId.Value);
                    if (userExists)
                    {
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

                        if (model.EmergencyNeedId.HasValue)
                        {
                            _context.EmergencyPurchaseRecords.Add(new EmergencyPurchaseRecord
                            {
                                UserOrderId = order.UserOrderId,
                                EmergencyNeedId = model.EmergencyNeedId.Value,
                                SupplyName = model.SupplyName,
                                Quantity = model.Quantity,
                                UnitPrice = model.TotalPrice / model.Quantity,
                                PurchaseDate = DateTime.Now,
                                CaseId = model.CaseId ?? 0,
                                PaymentMethod = model.PaymentMethod
                            });
                        }
                        else if (model.SupplyId.HasValue)
                        {
                            _context.UserOrderDetails.Add(new UserOrderDetail
                            {
                                UserOrderId = order.UserOrderId,
                                SupplyId = model.SupplyId.Value,
                                Quantity = model.Quantity,
                                UnitPrice = model.TotalPrice / model.Quantity,
                                OrderSource = "regular",
                                EmergencyNeedId = null
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new ProcessPaymentResult
            {
                Success = true,
                Order = order,
                OrderNumber = orderNumber
            };
        }

        public async Task<UserOrder?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.UserOrders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<UserOrder?> GetOrderByIdForUserAsync(int orderId, int userId)
        {
            return await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .FirstOrDefaultAsync(o => o.UserOrderId == orderId && o.UserId == userId);
        }

        public async Task UpdateOrderNumberAsync(UserOrder order, string newOrderNumber)
        {
            order.OrderNumber = newOrderNumber;
            order.PaymentStatus = "待付款";
            await _context.SaveChangesAsync();
        }

        public async Task<OrderResultViewModel?> GetEcpaySuccessViewModelAsync(string merchantTradeNo)
        {
            var order = await _context.UserOrders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderNumber == merchantTradeNo);

            if (order == null) return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == order.UserId);
            var donorName = user?.Name ?? "匿名捐贈者";

            var supplyName = "物資捐贈";
            var quantity = 0;
            var isEmergency = false;
            int? caseId = null;

            if (order.OrderDetails.Any())
            {
                if (order.OrderSource == "package")
                {
                    var firstDetail = order.OrderDetails.FirstOrDefault();
                    if (firstDetail != null)
                    {
                        var baseQuantity = firstDetail.SupplyId switch
                        {
                            14 => 2,
                            3 => 3,
                            16 => 1,
                            _ => 1
                        };
                        quantity = firstDetail.Quantity / baseQuantity;
                        supplyName = firstDetail.SupplyId switch
                        {
                            14 => "醫療照護套組",
                            3 => "營養食物套組",
                            16 => "清潔護理套組",
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
                                supplyNames.Add($"{supply.SupplyName}({detail.Quantity}份)");
                        }
                    }

                    supplyName = supplyNames.Count > 2
                        ? $"多項物資 (共{quantity}份)"
                        : string.Join("、", supplyNames);
                }
                else
                {
                    var orderDetail = order.OrderDetails.First();
                    quantity = orderDetail.Quantity;

                    if (orderDetail.EmergencyNeedId.HasValue)
                    {
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
                        var supply = await _context.Supplies
                            .FirstOrDefaultAsync(s => s.SupplyId == orderDetail.SupplyId);
                        if (supply != null)
                            supplyName = supply.SupplyName ?? "一般物資";
                    }
                }
            }

            if (order.EmergencyNeedId.HasValue && !isEmergency)
            {
                var emergencyRecord = await _context.EmergencyPurchaseRecords
                    .FirstOrDefaultAsync(e => e.UserOrderId == order.UserOrderId);

                if (emergencyRecord != null)
                {
                    supplyName = emergencyRecord.SupplyName ?? "緊急物資";
                    quantity = emergencyRecord.Quantity;
                    isEmergency = true;
                    caseId = emergencyRecord.CaseId;
                }
                else
                {
                    var emergency = await _context.EmergencySupplyNeeds
                        .FirstOrDefaultAsync(e => e.EmergencyNeedId == order.EmergencyNeedId.Value);
                    if (emergency != null)
                    {
                        supplyName = emergency.SupplyName ?? "緊急物資";
                        quantity = 1;
                        isEmergency = true;
                        caseId = emergency.CaseId;
                    }
                }
            }

            return new OrderResultViewModel
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
        }

        private static string GenerateOrderNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = Random.Shared.Next(100, 999);
            return $"NGO{timestamp}{random}";
        }

        private static List<(int supplyId, int quantity)> GetPackageItems(string packageType)
        {
            return packageType switch
            {
                "medical" => new List<(int, int)> { (14, 2), (15, 2), (18, 2), (19, 5) },
                "food" => new List<(int, int)> { (3, 3), (4, 4), (5, 6), (7, 12) },
                "hygiene" => new List<(int, int)> { (25, 1), (26, 1), (27, 4), (28, 5) },
                _ => new List<(int, int)>()
            };
        }
    }
}
