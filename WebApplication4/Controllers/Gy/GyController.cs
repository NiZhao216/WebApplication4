using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers.Gy
{
/// <summary>
/// 自定义控制器类 GyController，继承自 Controller 基类
/// </summary>
    public class GyController : Controller
    {
    /// <summary>
    /// 默认的 Index 动作方法
    /// </summary>
    /// <returns>返回对应的视图</returns>
        public IActionResult Index()
        {
        // 调用 View() 方法返回默认视图
            return View();
        }
    }
}
