using Microsoft.AspNetCore.Mvc;

namespace HaloAxis_UI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
