using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// 1. 仅替换：移除 SQL Server 驱动，添加 MySQL 驱动
using MySqlConnector;
using System.Security.Claims;

namespace WebApplication4.Controllers.Login
{
    public class LoginController : Controller
    {
        // 2. 仅替换：修改为 MySQL 连接字符串（根据你的 MySQL 环境调整账号密码）
        public string constr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public IActionResult Index()
        {
            // 未改动
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            // 3. 仅替换：SqlConnection → MySqlConnection
            using (var conn = new MySqlConnection(constr))
            {
                conn.Open();
                string sql = "SELECT role FROM userstable WHERE username = @username AND pwd = @password";
                // 4. 仅替换：SqlCommand → MySqlCommand
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 未改动：参数化查询逻辑完全保留
                    cmd.Parameters.AddWithValue("@username", username ?? string.Empty);
                    cmd.Parameters.AddWithValue("@password", password ?? string.Empty);
                    object result = cmd.ExecuteScalar();
                    

                    if (result!=null)
                    {
                        // 未改动：Claims 认证逻辑完全保留
                        var claims = new List<Claim> 
                        { 
                            new Claim(ClaimTypes.Name, username) ,
                            new Claim(ClaimTypes.Role,result.ToString() )
                        };
                        var identity = new ClaimsIdentity(claims, "Cookies");
                        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // 未改动：错误提示逻辑完全保留
                        ViewBag.ErrorMsg = "用户名或密码错误";
                        return View();
                    }
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 1. 清除当前用户的登录Cookie（退出核心）
            await HttpContext.SignOutAsync();

            // 2. 退出后跳转到 登录页面
            return RedirectToAction("Index", "Login");
        }
    }
}