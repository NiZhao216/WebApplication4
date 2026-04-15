// Controllers/Admin/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using MySqlConnector;
namespace WebApplication4.Controllers.Admin
{
    [Authorize(Roles = "管理员")]
    public class UsersManagement : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            List<User> users = await GetUsers(pageIndex); // 获取第一页用户数据
            return View(users);
        }
        public IActionResult  UsersF5(int pageIndex)
        {
            return RedirectToAction("Index", new { pageIndex = pageIndex });
        }
        public async Task<List<User>> GetUsers(int pageIndex)
        {
            // 固定配置
        const int pageSize = 20;
        // 分页偏移量公式
        int offset = (pageIndex - 1) * pageSize;
        List<User> userList = new List<User>();
        // using：自动关闭数据库连接（企业强制规范）
        using (MySqlConnection conn = new MySqlConnection(_conStr))
        {
            // SQL：过滤管理员 + 排序 + 分页
            string sql = @"
                SELECT userid, username, pwd, role
                FROM userstable
                WHERE role <> '管理员'
                ORDER BY userid DESC
                LIMIT @Offset, @PageSize;";
            await conn.OpenAsync();
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                // 标准参数化（防SQL注入，比AddWithValue更规范）
                cmd.Parameters.Add("@Offset", MySqlDbType.Int32).Value = offset;
                cmd.Parameters.Add("@PageSize", MySqlDbType.Int32).Value = pageSize;

                try
                {
                    // 读取数据
                    using (MySqlDataReader reader =await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            User user = new User();
                            // 安全取值，避免空值报错
                            user.userid = reader.GetInt32("userid");
                            user.username = reader.GetString("username") ?? "";
                            user.pwd = reader.GetString("pwd") ?? "";
                            userList.Add(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 企业级：这里替换为日志记录（如Log4net/NLog）
                    Console.WriteLine($"查询用户分页失败：{ex.Message}");
                }
            }
        }

        return userList;
        }

    }
}
