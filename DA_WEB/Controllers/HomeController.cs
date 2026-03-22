using Azure.Core.Pipeline;
using DA_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DA_WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult PrivacyPolicy()
        {
            return View(); // Trả về Views/Home/PrivacyPolicy.cshtml
        }
        public IActionResult TermsOfService()
        {
            return View(); // Trả về Views/Home/TermsOfService.cshtml
        }
        public IActionResult ReturnPolicy()
        {
            return View(); // Trả về Views/Home/TermsOfService.cshtml
        }

        public IActionResult ShippingPolicy()
        {
            return View();
        }
        public IActionResult SizeGuide()
        {
            return View(); // Trả về Views/Home/SizeGuide.cshtml
        }
        public IActionResult Contact()
        {
            return View(); // Trả về Views/Home/Contact.cshtml
        }
        public IActionResult Philosophy()
        {
            return View(); // Trả về Views/Home/Philosophy.cshtml
        }
        public IActionResult About()
        {
            return View(); // Trả về Views/Home/About.cshtml
        }
        public IActionResult FAQ()
        {
            return View(); // Trả về Views/Home/FAQ.cshtml
        }
    }
}
