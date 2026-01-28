using Microsoft.AspNetCore.Mvc;
// 民眾 個案 報名活動
namespace NGOPlatformWeb.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "民眾活動頁面展示";
            return View();
        }
    }
}
