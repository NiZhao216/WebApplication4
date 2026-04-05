using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;

namespace WebApplication4.Controllers.Catlist
{
    public class CatlistController : Controller
    {

        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        // 猫咪列表页（纯原生，无ASP）
        public async Task<IActionResult> Index()
        {
            string username = User.Identity?.Name ?? "";
            List<Cats> list = new List<Cats>();

            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                // 不查ID，只查3个字段
                string sql = "SELECT catname,gender,breed FROM userscat WHERE username=@Username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Cats
                            {
                                CatName = reader.GetString(0),
                                Gender = reader.GetString(1),
                                Breed = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return View(list);
        }
        public async Task<IActionResult> Delete(string catName)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM userscat WHERE username=@Username AND catname=@CatName";
                MySqlCommand cmd=new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", User.Identity?.Name ?? "");
                cmd.Parameters.AddWithValue("@CatName", catName);
                int num =await cmd.ExecuteNonQueryAsync();
                if (num > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                    {
                    // 删除失败，返回错误信息
                    TempData["ErrorMessage"] = "删除失败，请重试。";
                    return RedirectToAction("Index");
                }
            }
        }
        public async Task<IActionResult> InCreatedex()
        {
            return RedirectToAction("InCreatedex", "Cat");
        }
    }    
        
}
