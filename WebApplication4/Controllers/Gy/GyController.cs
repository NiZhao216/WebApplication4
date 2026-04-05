using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers.Gy
{
    public class GyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
