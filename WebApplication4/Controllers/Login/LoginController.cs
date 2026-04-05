using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication4.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApplication4.Controllers.Login
{
    public class LoginController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public IActionResult Index()
        {
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
                    string sql = "SELECT pwd, role FROM userstable WHERE username = @username";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedHash = reader["pwd"]?.ToString() ?? string.Empty;
                                string role = reader["role"]?.ToString() ?? string.Empty;

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
                                        new Claim(ClaimTypes.Role, role),
                                        new Claim("logintoken", token)
                                    };

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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            string? username = User.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    using (var conn = new MySqlConnection(_conStr))
                    {
                        await conn.OpenAsync();
                        string sql = "UPDATE userstable SET token = null WHERE username = @username";
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch
                {
                }
            }
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Login");
        }

        private async Task<User> GetUserInfoByUsername(string username)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
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
                                id = (int)reader["id"],
                                avatar = reader["Avatar"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null!;
        }
    }
}