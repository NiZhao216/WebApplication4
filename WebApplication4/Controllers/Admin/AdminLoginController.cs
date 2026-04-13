using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication4.Controllers.AdminLogin
{
    /// <summary>
    /// 登录控制器，处理用户登录、登出及用户信息获取功能
    /// </summary>
    [AllowAnonymous]
    public class AdminLoginController : Controller
    {
        // 数据库连接字符串，用于连接MySQL数据库
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        /// <summary>
        /// 显示登录页面
        /// 如果用户已登录，则重定向到首页
        /// </summary>
        /// <returns>登录页面视图或首页重定向</returns>
        
        public IActionResult Index()
        {

            return View("~/Views/Admin/Login.cshtml");
        }
        public async Task<IActionResult> Login(string username, string password)
        {
          
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "用户名和密码不能为空");
                return View("~/Views/Admin/Login.cshtml");
            }
            else
            {
                // 检查用户名和密码是否正确
                var res = await CheckLogin(username, password);
                if (res)
                {
                    // 创建身份标识
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),      
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ModelState.AddModelError("", "用户名或密码错误");
                    return View("~/Views/Admin/Login.cshtml");
                }
            }
        }
        public async Task<bool> CheckLogin(string username, string password)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT role FROM admin WHERE username = @username AND password = @password";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                bool result = (bool)await cmd.ExecuteScalarAsync();
                return result;
            }
        }
    }
}
      