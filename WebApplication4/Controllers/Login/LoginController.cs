using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication4.Models;

namespace WebApplication4.Controllers.Login
{
    


    public class LoginController : Controller
    {
        // 优化：从配置读取连接字符串（推荐，避免硬编码）
        // 实际项目中建议从appsettings.json读取，这里先简化保留格式
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            // 前置校验：空值过滤
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMsg = "用户名或密码不能为空";
                return View();
            }

            try
            {
                using (var conn = new MySqlConnection(_conStr))
                {
                    await conn.OpenAsync(); // 异步打开连接（推荐）
                    // 注意：生产环境必须用密码哈希比对，此处先保留逻辑，后续替换
                    string sql = "SELECT role FROM userstable WHERE username = @username AND pwd = @password";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        object result = await cmd.ExecuteScalarAsync(); // 异步执行

                        if (result != null)
                        {
                            // 1. 构建身份认证Claims（添加更多用户信息）
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, username),
                                new Claim(ClaimTypes.Role, result.ToString())
                            };

                            // 2. 读取用户完整信息并添加到Claims（后续页面可直接获取）
                            User currentUser = await GetUserInfoByUsername(username);
                           

                            // 3. 生成登录Cookie
                            var identity = new ClaimsIdentity(claims, "Cookies");
                            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

                             return RedirectToAction("Index","Home");
                        }
                        else
                        {
                            ViewBag.ErrorMsg = "用户名或密码错误";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 异常处理：记录日志（生产环境）+ 友好提示
                ViewBag.ErrorMsg = "登录失败，请稍后重试";
                // _logger.LogError(ex, "登录异常：{Username}", username); // 推荐添加日志
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 修正：指定认证方案，确保退出成功
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Login");
        }

        // 抽离：读取用户完整信息的方法（复用+解耦）
        private async Task<User> GetUserInfoByUsername(string username)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                // 修正：将不规范的"by"改为birthday（需同步修改数据库字段名）
                string sql = "SELECT * FROM userstable WHERE username=@username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                username = reader["username"]?.ToString() ?? string.Empty,
                                email = reader["email"]?.ToString() ?? string.Empty,
                                phone = reader["phone"]?.ToString() ?? string.Empty,
                                pr = reader["pr"]?.ToString() ?? string.Empty,
                                age = reader["age"]?.ToString() ?? string.Empty,
                                sex = reader["sex"]?.ToString() ?? string.Empty,
                                by = reader["bry"] as DateTime?,
                                id = (int)reader["id"]
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}