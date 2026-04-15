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

namespace WebApplication4.Controllers.Login
{
    /// <summary>
    /// 登录控制器，处理用户登录、登出及用户信息获取功能
    /// </summary>
    [AllowAnonymous]
    public class LoginController : Controller
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
            // 检查用户是否已认证，如果已认证则重定向到首页
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMsg = "用户名或密码不能为空";
                return View();
            }

            try
            {
                using (var conn = new MySqlConnection(_conStr))
                {
                    await conn.OpenAsync();

                    // 先按用户名取出存储的哈希和角色
                    string sql = "SELECT pwd FROM userstable WHERE username = @username";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedHash = reader["pwd"]?.ToString() ?? string.Empty;
                                
                               
                                var hasher = new PasswordHasher<object>();
                                var verifyResult = hasher.VerifyHashedPassword(null, storedHash, password);

                                if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                                {
                                    // 在需要 rehash 时更新数据库（可选但推荐）
                                    if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                                    {
                                        string newHash = hasher.HashPassword(null, password);
                                        reader.Close();
                                        string updSql = "UPDATE userstable SET pwd = @newHash WHERE username = @username";
                                        using (var upcmd = new MySqlCommand(updSql, conn))
                                        {
                                            upcmd.Parameters.AddWithValue("@newHash", newHash);
                                            upcmd.Parameters.AddWithValue("@username", username);
                                            await upcmd.ExecuteNonQueryAsync();
                                        }
                                    }

                                    // 生成 token 并更新数据库
                                    string token = Guid.NewGuid().ToString();
                                    // reader may still be open if not rehash; ensure closed before update
                                    if (!reader.IsClosed) reader.Close();
                                    string sql2 = "UPDATE userstable SET token=@token WHERE username=@username";
                                    using (MySqlCommand tocmd = new MySqlCommand(sql2, conn))
                                    {
                                        tocmd.Parameters.AddWithValue("@token", token);
                                        tocmd.Parameters.AddWithValue("@username", username);
                                        int num = await tocmd.ExecuteNonQueryAsync();
                                        if (!(num > 0))
                                        {
                                            return View();
                                        }
                                    }

                                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.Name, username),
                                        new Claim(ClaimTypes.Role, "用户"),
                                        new Claim("logintoken", token)
                                    };

                                    var identity = new ClaimsIdentity(claims, "UserAuth");
                                    await HttpContext.SignInAsync("UserAuth", new ClaimsPrincipal(identity));
                                   
                                    return RedirectToAction("Index","Home");
                                }
                                else
                                {
                                    ViewBag.ErrorMsg = "用户名或密码错误";
                                    return View();
                                }
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "用户名或密码错误";
                                return View();
                            }
                        }
                    }
                }
            }
            catch
            {
                ViewBag.ErrorMsg = "登录失败，请稍后重试";
                return View();
            }
        }

        /// <summary>
        /// 处理用户登出请求的HTTP POST方法
        /// </summary>
        /// <returns>重定向到登录页面</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 获取当前登录用户的用户名
            string? username = User.Identity?.Name;

            // 如果用户名不为空
            if (!string.IsNullOrEmpty(username))
            {
                
                    // 创建数据库连接
                    using (var conn = new MySqlConnection(_conStr))
                    {
                        // 异步打开数据库连接
                        await conn.OpenAsync();
                        // 准备SQL语句，用于将用户的token设置为null
                        string sql = "UPDATE userstable SET token = null WHERE username = @username";
                        // 创建并执行命令
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            // 添加参数以防止SQL注入
                            cmd.Parameters.AddWithValue("@username", username);
                            // 异步执行SQL命令
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                

            }
            // 异步执行Cookie登出
            await HttpContext.SignOutAsync("UserAuth");
            // 重定向到登录页面的Index动作
            return RedirectToAction("Index", "Login");
        }

        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        /// <param name="username">要查询的用户名</param>
        /// <returns>返回User对象，如果未找到则返回null</returns>
        private async Task<User> GetUserInfoByUsername(string username)
        {
            // 使用using语句确保数据库连接在使用后被正确关闭
            using (var conn = new MySqlConnection(_conStr))
            {
                // 异步打开数据库连接
                await conn.OpenAsync();
                // 定义SQL查询语句，使用参数化查询防止SQL注入
                string sql = "SELECT * FROM userstable WHERE username=@username";
                // 使用using语句确保命令对象在使用后被正确释放
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 向SQL命令中添加参数
                    cmd.Parameters.AddWithValue("@username", username);
                    // 使用using语句确保数据读取器在使用后被正确关闭
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 异步读取数据
                        if (await reader.ReadAsync())
                        {
                            // 读取数据并创建User对象
                            return new User
                            {
                                username = reader["username"]?.ToString() ?? string.Empty,
                                email = reader["email"]?.ToString() ?? string.Empty,
                                phone = reader["phone"]?.ToString() ?? string.Empty,
                                pr = reader["pr"]?.ToString() ?? string.Empty,
                                age = reader["age"]?.ToString() ?? string.Empty,
                                sex = reader["sex"]?.ToString() ?? string.Empty,
                                by = reader["bry"] as DateTime?,
                                userid = (int)reader["userid"],
                                avatar = reader["Avatar"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            // 如果未找到用户，返回null
            return null!;
        }
    }
}