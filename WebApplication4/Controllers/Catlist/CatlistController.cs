using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;

namespace WebApplication4.Controllers.Catlist
{
    public class CatlistController : Controller
    {

        // 数据库连接字符串，用于连接MySQL数据库
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        /// <summary>
        /// 处理首页请求的异步方法
        /// </summary>
        /// <returns>返回包含猫咪列表的视图</returns>
        public async Task<IActionResult> Index()
        {
            // 获取当前登录用户的用户名，如果未登录则为空字符串
            string username = User.Identity?.Name ?? "";
            // 创建猫咪列表集合，用于存储查询到的猫咪信息
            List<Cats> list = new List<Cats>();

            // 使用using语句确保数据库连接正确关闭，自动释放资源
            using (var conn = new MySqlConnection(_conStr))
            {
                // 异步打开数据库连接，避免阻塞主线程
                await conn.OpenAsync();
                // 不查ID，只查3个字段
                string sql = "SELECT catname,gender,breed,avatar FROM userscat WHERE username=@Username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 添加查询参数，防止SQL注入
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 逐行读取查询结果
                        while (await reader.ReadAsync())
                        {
                            // 将读取的数据添加到猫咪列表中
                            list.Add(new Cats
                            {
                                CatName = reader.GetString(0),
                                Gender = reader.GetString(1),
                                Breed = reader.GetString(2),
                                avatar = reader.IsDBNull(3) ? "" : reader.GetString(3)

                            });
                        }
                    }
                }
            }
            // 返回包含猫咪列表的视图
            return View(list);
        }
        /// <summary>
        /// 删除用户分类的方法
        /// </summary>
        /// <param name="catName">要删除的分类名称</param>
        /// <returns>返回ActionResult，重定向到Index视图</returns>
        public async Task<IActionResult> Delete(string catName)
        {
            // 使用using语句确保数据库连接在使用后被正确关闭
            using (var conn = new MySqlConnection(_conStr))
            {
                // 异步打开数据库连接
                await conn.OpenAsync();
                // 定义SQL删除语句，使用参数化查询防止SQL注入
                string sql = "DELETE FROM userscat WHERE username=@Username AND catname=@CatName";
                // 创建SqlCommand对象
                MySqlCommand cmd=new MySqlCommand(sql, conn);
                // 添加参数，使用当前用户名和传入的分类名
                cmd.Parameters.AddWithValue("@Username", User.Identity?.Name ?? "");
                cmd.Parameters.AddWithValue("@CatName", catName);
                // 执行SQL删除语句，返回受影响的行数
                int num =await cmd.ExecuteNonQueryAsync();
                // 判断是否成功删除（受影响的行数大于0）
                if (num > 0)
                {
                    // 删除成功，重定向到Index视图
                    return RedirectToAction("Index");
                }
                else
                    {
                    // 删除失败，返回错误信息
                    TempData["Msg"] = "删除失败，请重试。";
                    return RedirectToAction("Index");
                }
            }
        }
        /// <summary>
        /// 异步重定向到Cat控制器的InCreatedex方法
        /// </summary>
        /// <returns>返回一个重定向到Cat控制器InCreatedex方法的ActionResult</returns>
        public async Task<IActionResult> InCreatedex()
        {
            // 使用RedirectAction方法重定向到Cat控制器的InCreatedex方法
            return RedirectToAction("InCreatedex", "Cat");
        }
    }    
        
}
