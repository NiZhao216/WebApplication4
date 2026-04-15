using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApplication4.Models;
using MySqlConnector;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication4.Controllers.DoctorLogin
{
    [AllowAnonymous]
    public class DoctorLoginController : Controller
    {   
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        public IActionResult Index()
        {
            return View("~/Views/Doctor/Login.cshtml");
        }
        public async Task<IActionResult> Login(string id, string pwd)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pwd))
            {
                ModelState.AddModelError("", "用户名和密码不能为空");
                return View("~/Views/Doctor/Login.cshtml");
            }
            else
            {
                
                if (await CheckLogin(id, pwd))
                {
                    // 生成登录令牌（可选）
                    string token = Guid.NewGuid().ToString();
                    // 存储令牌到数据库（示例，实际应加密存储
                    await SaveTokenToDatabase(id, token);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, id),
                        new Claim(ClaimTypes.Role, "医生"),
                        new Claim("loginToken", token)                  
                    };

                    var identity = new ClaimsIdentity(claims, "DoctorAuth");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("DoctorAuth", principal);
                    return RedirectToAction("Index", "DoctorHome");
                }
                else
                {
                    ModelState.AddModelError("", "用户名或密码错误");
                    return View("~/Views/Doctor/Login.cshtml");
                }
            }
        }

        public async Task<bool> CheckLogin(string id, string pwd)
        {
            using (MySqlConnection conn = new MySqlConnection(_conStr))
            {
                string sql = "SELECT Count(*) FROM doctors WHERE did = @id AND pwd = @pwd";
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pwd", pwd);
                    bool result = (long)await cmd.ExecuteScalarAsync() > 0;
                    return result;
                }
            }
         }
        public async Task SaveTokenToDatabase(string id, string token)
        {
            using (MySqlConnection conn = new MySqlConnection(_conStr))
            {
                string sql = "UPDATE doctors SET token = @token WHERE did = @id";
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@token", token);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        
    }
}