using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;

namespace WebApplication4.Controllers.Zc
{
    public class ZcController : Controller
    {
        public string constr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password, string password1)
        {
            if (password1 == null || password == null || username == null)
            {
                ViewBag.Message = "请完整输入注册信息";
                return View();
            }
            if (username.Contains(' '))
            {
                ViewBag.Message = "名字不能包含空格";
                return View();
            }
            if (password != password1)
            {
                ViewBag.Message = "两次输入的密码不一致";
                return View();
            }

            using (var conn = new MySqlConnection(constr))
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM userstable WHERE username = @username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    int num = Convert.ToInt32(result);
                    if (num > 0)
                    {
                        ViewBag.Message = "用户名已存在";
                        return View();
                    }
                }

                // 使用 PasswordHasher 对密码进行哈希后存储（不存明文）
                var hasher = new PasswordHasher<object>();
                string hashed = hasher.HashPassword(null, password);

                string insertSql = "INSERT INTO userstable(username, pwd) VALUES(@username, @password)";
                using (var insertCmd = new MySqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@username", username);
                    insertCmd.Parameters.AddWithValue("@password", hashed);
                    int rows = insertCmd.ExecuteNonQuery();
                    if (rows == 1)
                    {
                        TempData["Message"] = "注册成功";
                        return RedirectToAction("Index", "Login");
                    }
                    else
                    {
                        ViewBag.Message = "注册失败";
                        return View();
                    }
                }
            }
        }
    }
}