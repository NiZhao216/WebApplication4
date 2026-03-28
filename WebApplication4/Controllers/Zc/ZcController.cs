using Microsoft.AspNetCore.Mvc;
// 1. 仅替换：移除 SQL Server 驱动，添加 MySQL 驱动
using MySqlConnector;
using System.Xml.Linq;

namespace WebApplication4.Controllers.Zc
{
    public class ZcController : Controller
    {
        // 2. 仅替换：修改为 MySQL 连接字符串（和登录控制器保持一致）
        public string constr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        [HttpGet]
        public IActionResult Index()
        {
            // 未改动
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password, string password1)
        {
            // 未改动：所有参数验证逻辑完全保留
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

            // 3. 仅替换：SqlConnection → MySqlConnection
            using (var conn = new MySqlConnection(constr))
            {
                conn.Open();

                // 检查用户名是否存在
                string sql = "SELECT COUNT(*) FROM userstable WHERE username = @username";
                // 4. 仅替换：SqlCommand → MySqlCommand
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 未改动：参数化查询逻辑保留
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    int num = Convert.ToInt32(result);
                    if (num > 0)
                    {
                        ViewBag.Message = "用户名已存在";
                        return View();
                    }
                }

                // 插入新用户
                string insertSql = "INSERT INTO userstable(username, pwd) VALUES(@username, @password)";
                // 4. 仅替换：SqlCommand → MySqlCommand
                using (var insertCmd = new MySqlCommand(insertSql, conn))
                {
                    // 未改动：参数逻辑保留
                    insertCmd.Parameters.AddWithValue("@username", username);
                    insertCmd.Parameters.AddWithValue("@password", password);
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