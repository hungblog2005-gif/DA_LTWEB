using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DA_WEB.Models;

namespace DA_WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]   // ← chỉ Admin mới vào được
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}