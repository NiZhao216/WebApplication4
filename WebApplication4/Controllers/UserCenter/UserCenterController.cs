using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication4.Controllers.UserCenter
{
    [Authorize]
    public class UserCenterController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Prefill common fields from claims (if present)
            ViewBag.Username = User.Identity?.Name ?? string.Empty;
            ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("Email")?.Value ?? string.Empty;
            ViewBag.Phone = User.FindFirst("Phone")?.Value ?? string.Empty;
            ViewBag.Age = User.FindFirst("Age")?.Value ?? string.Empty;
            ViewBag.Sex = User.FindFirst("Sex")?.Value ?? string.Empty;
            ViewBag.By = User.FindFirst("by")?.Value ?? string.Empty;
            ViewBag.Pr = User.FindFirst("Pr")?.Value ?? string.Empty;
            ViewBag.Address = User.FindFirst("Address")?.Value ?? string.Empty;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, string username, string pwd, string email, string phone, string age, string sex, string by, string pr, string address)
        {
            // Basic server-side validation (can be expanded)
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Message"] = "用户名不能为空";
                return RedirectToAction("Index");
            }

            // TODO: Persist changes to database. For now, simulate async work and return success.
            await Task.CompletedTask;

            TempData["Message"] = "保存成功";
            return RedirectToAction("Index");
        }
    }
}
