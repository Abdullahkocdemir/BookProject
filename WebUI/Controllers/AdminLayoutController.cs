using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Deneme()
        {
            return View();
        }
    }
}
