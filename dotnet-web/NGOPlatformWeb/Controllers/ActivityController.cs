using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Services;
using System.Security.Claims;

public class ActivityController : Controller
{
    private readonly NGODbContext _context;
    private readonly IActivityService _activityService;

    public ActivityController(NGODbContext context, IActivityService activityService)
    {
        _context = context;
        _activityService = activityService;
    }

    // 未登入用戶專用路由
    public async Task<IActionResult> PublicIndex(string? category, string? keyword)
    {
        try
        {
            var viewModel = await _activityService.GetActivitiesForRoleAsync("Guest", category, keyword);
            return View("ActivityIndex", viewModel);
        }
        catch (Exception ex)
        {
            return Content("抓資料時發生錯誤：" + ex.Message);
        }
    }

    // 一般用戶專用路由
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UserIndex(string? category, string? keyword)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var viewModel = await _activityService.GetActivitiesForRoleAsync("User", userId, category, keyword);
            return View("ActivityIndex", viewModel);
        }
        catch (Exception ex)
        {
            return Content("抓資料時發生錯誤：" + ex.Message);
        }
    }

    // 個案用戶專用路由
    [Authorize(Roles = "Case")]
    public async Task<IActionResult> CaseIndex(string? category, string? keyword)
    {
        try
        {
            var caseId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var viewModel = await _activityService.GetActivitiesForRoleAsync("Case", caseId, category, keyword);
            return View("ActivityIndex", viewModel);
        }
        catch (Exception ex)
        {
            return Content("抓資料時發生錯誤：" + ex.Message);
        }
    }

    // 🔄 智能重導向 (保持向後兼容)
    public IActionResult CaseActivityIndex(string? category, string? keyword)
    {
        var userRole = _activityService.DetermineUserRole(User);

        return userRole switch
        {
            "User" => RedirectToAction("UserIndex", new { category, keyword }),
            "Case" => RedirectToAction("CaseIndex", new { category, keyword }),
            _ => RedirectToAction("PublicIndex", new { category, keyword })
        };
    }

    // 取消報名功能 (使用分層架構)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CancelSignup(int id)
    {
        try
        {
            var userRole = _activityService.DetermineUserRole(User);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var success = await _activityService.CancelRegistrationAsync(userId, id, userRole);

            if (success)
            {
                TempData["CancelSuccess"] = "true";
                TempData["CancelMessage"] = "已成功取消報名！";
            }
            else
            {
                TempData["ErrorMessage"] = "取消報名失敗，請稍後再試。";
            }

            // 根據用戶角色重導向
            return userRole switch
            {
                "User" => RedirectToAction("UserIndex"),
                "Case" => RedirectToAction("CaseIndex"),
                _ => RedirectToAction("PublicIndex")
            };
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "取消報名時發生錯誤：" + ex.Message;
            return RedirectToAction("PublicIndex");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Case")]
    public IActionResult CaseSignup(int id)
    {
        var activity = _context.Activities.FirstOrDefault(a => a.ActivityId == id);
        if (activity == null) return NotFound();

        var caseIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!int.TryParse(caseIdClaim, out int caseId))
            return RedirectToAction("Login", "Auth");

        var viewModel = new CaseSignupViewModel
        {
            Activity = activity,
            Registration = new CaseActivityRegistrations
            {
                ActivityId = activity.ActivityId,
                CaseId = caseId,
                Status = "registered"
            }
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Case")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaseSignup(CaseSignupViewModel vm)
    {
        if (vm == null || vm.Registration == null)
            return BadRequest("資料不完整");

        var caseIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!int.TryParse(caseIdClaim, out int caseId))
            return RedirectToAction("Login", "Auth");

        // 強制使用 Claims 的 CaseId，防止前端偽造
        vm.Registration.CaseId = caseId;
        vm.Registration.RegisterTime = DateTime.Now;

        try
        {
            _context.CaseActivityRegistrations.Add(vm.Registration);
            await _context.SaveChangesAsync();
            TempData["SignupSuccess"] = true;
            return RedirectToAction("CaseActivityIndex");
        }
        catch (Exception ex)
        {
            vm.Activity = _context.Activities!
                .FirstOrDefault(a => a.ActivityId == vm.Registration!.ActivityId);
            ViewBag.Error = ex.Message;
            return View(vm);
        }
    }

    // API: 取得活動資訊和剩餘名額
    [HttpGet]
    [Authorize(Roles = "User,Case")]
    public async Task<IActionResult> GetActivityInfo(int id)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                return Json(new { success = false, message = "活動不存在" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userRole = _activityService.DetermineUserRole(User);
            var registeredIds = await _activityService.GetUserRegisteredActivityIdsAsync(userId, userRole);
            var isRegistered = registeredIds.Contains(id);

            var availableSlots = activity.MaxParticipants - activity.CurrentParticipants;
            var maxCompanions = userRole == "Case" ? 0 : Math.Max(0, availableSlots - 1); // Case 不能攜帶同伴

            return Json(new
            {
                success = true,
                activityName = activity.ActivityName,
                currentParticipants = activity.CurrentParticipants,
                maxParticipants = activity.MaxParticipants,
                availableSlots = availableSlots,
                maxCompanions = maxCompanions,
                isRegistered = isRegistered
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // API: User 報名 (支援同伴人數)
    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterWithCompanions(int activityId, int numberOfCompanions)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var success = await _activityService.RegisterUserWithCompanionsAsync(userId, activityId, numberOfCompanions);

            if (success)
            {
                // 取得更新後的活動資訊
                var activity = await _activityService.GetActivityByIdAsync(activityId);
                
                return Json(new { 
                    success = true, 
                    message = "報名成功！",
                    currentParticipants = activity?.CurrentParticipants ?? 0,
                    maxParticipants = activity?.MaxParticipants ?? 0
                });
            }
            else
            {
                return Json(new { success = false, message = "報名失敗，可能名額不足或已經報名過。" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "報名時發生錯誤：" + ex.Message });
        }
    }

    // API: 取消報名 (AJAX版本)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRegistrationAjax(int activityId)
    {
        try
        {
            var userRole = _activityService.DetermineUserRole(User);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var success = await _activityService.CancelRegistrationAsync(userId, activityId, userRole);

            if (success)
            {
                // 取得更新後的活動資訊
                var activity = await _activityService.GetActivityByIdAsync(activityId);
                
                return Json(new { 
                    success = true, 
                    message = "已成功取消報名！",
                    currentParticipants = activity?.CurrentParticipants ?? 0,
                    maxParticipants = activity?.MaxParticipants ?? 0
                });
            }
            else
            {
                return Json(new { success = false, message = "取消報名失敗，請稍後再試。" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "取消報名時發生錯誤：" + ex.Message });
        }
    }

    // API: Case 報名 (固定1人)
    [HttpPost]
    [Authorize(Roles = "Case")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCase(int activityId)
    {
        try
        {
            var caseId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var success = await _activityService.RegisterCaseAsync(caseId, activityId);

            if (success)
            {
                // 取得更新後的活動資訊
                var activity = await _activityService.GetActivityByIdAsync(activityId);
                
                return Json(new { 
                    success = true, 
                    message = "報名成功！",
                    currentParticipants = activity?.CurrentParticipants ?? 0,
                    maxParticipants = activity?.MaxParticipants ?? 0
                });
            }
            else
            {
                return Json(new { success = false, message = "報名失敗，可能名額不足或已經報名過。" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "報名時發生錯誤：" + ex.Message });
        }
    }
}
