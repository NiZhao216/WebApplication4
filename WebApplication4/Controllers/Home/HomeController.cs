using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;
namespace WebApplication4.Controllers.Home
{
    
    [Authorize(Roles ="普通用户")]
    public class HomeController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        public IActionResult Index()
        {  
            User currentUser = GetUserInfoByUsername(User.Identity?.Name ?? string.Empty).Result;
            return View(currentUser);
        }
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
                                id = (int)reader["id"],
                                avatar = reader["Avatar"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }

    }
}
