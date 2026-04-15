using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication4.Models;
namespace WebApplication4.Controllers.Order
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        public async Task<IActionResult> Index()
        {
            List<Orders> orders = await GetOrdersByUsername(User.Identity!.Name!);
            return View(orders);
        }
        public async Task<List<Orders>> GetOrdersByUsername(string username)
        {
            List<Orders> orders = new List<Orders>();
            using (var con = new MySqlConnection(_conStr))
            {
                await con.OpenAsync();
                using (var cmd = new MySqlCommand("SELECT * FROM orders WHERE UserName = @UserName", con))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Orders
                            {
                                Orid = reader["orid"] != DBNull.Value ? Convert.ToInt32(reader["orid"]) : 0,
                                UserName = reader["username"] != DBNull.Value ? reader["username"].ToString() : string.Empty,
                                CatId = reader["catid"] != DBNull.Value ? Convert.ToInt32(reader["catid"]) : 0,
                                CatName = reader["catname"] != DBNull.Value ? reader["catname"].ToString() : string.Empty,
                                Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty,
                                ServiceType = reader["servicetype"] != DBNull.Value ? reader["servicetype"].ToString() : string.Empty,
                                AppointmentTime = reader["appointmenttime"] != DBNull.Value ? Convert.ToDateTime(reader["appointmenttime"]) : DateTime.MinValue,
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : string.Empty,
                                DoctorId = reader["did"] != DBNull.Value ? Convert.ToInt32(reader["did"]) : 0,
                                DoctorName = reader["doctor_name"] != DBNull.Value ? reader["doctor_name"].ToString() : string.Empty,
                                Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : string.Empty,
                                CreateTime = reader["createtime"] != DBNull.Value ? Convert.ToDateTime(reader["createtime"]) : DateTime.MinValue,
                            });
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// 根据订单ID获取订单信息
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>返回一个Orders对象，包含订单的详细信息；如果未找到订单则返回null</returns>
        public async Task<Orders> GetOrderById(int orderId)
        {
            // 使用using语句确保数据库连接在使用后被正确关闭和释放
            using (var con = new MySqlConnection(_conStr))
            {
                // 异步打开数据库连接
                await con.OpenAsync();
                // 创建并配置SQL命令，使用参数化查询防止SQL注入
                using (var cmd = new MySqlCommand("SELECT * FROM orders WHERE orid = @Id", con))
                {
                    // 向命令中添加参数
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    // 使用using语句确保数据阅读器在使用后被正确关闭和释放
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 检查是否有数据可读
                        if (await reader.ReadAsync())
                        {
                            // 创建并填充Orders对象，处理可能的DBNull值
                            return new Orders
                            {
                                // 订单ID，如果值为DBNull则设为0
                                Orid = reader["orid"] != DBNull.Value ? Convert.ToInt32(reader["orid"]) : 0,
                                // 用户名，如果值为DBNull则设为空字符串
                                UserName = reader["username"] != DBNull.Value ? reader["username"].ToString() : string.Empty,
                                // 分类ID，如果值为DBNull则设为0
                                CatId = reader["catid"] != DBNull.Value ? Convert.ToInt32(reader["catid"]) : 0,
                                // 分类名称，如果值为DBNull则设为空字符串
                                CatName = reader["catname"] != DBNull.Value ? reader["catname"].ToString() : string.Empty,
                                // 电话号码，如果值为DBNull则设为空字符串
                                Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty,
                                // 服务类型，如果值为DBNull则设为空字符串
                                ServiceType = reader["servicetype"] != DBNull.Value ? reader["servicetype"].ToString() : string.Empty,
                                // 预约时间，如果值为DBNull则设为最小日期值
                                AppointmentTime = reader["appointmenttime"] != DBNull.Value ? Convert.ToDateTime(reader["appointmenttime"]) : DateTime.MinValue,
                                // 描述信息，如果值为DBNull则设为空字符串
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : string.Empty,
                                // 医生ID，如果值为DBNull则设为0
                                DoctorId = reader["did"] != DBNull.Value ? Convert.ToInt32(reader["did"]) : 0,
                                // 医生姓名，如果值为DBNull则设为空字符串
                                DoctorName = reader["doctor_name"] != DBNull.Value ? reader["doctor_name"].ToString() : string.Empty,
                                // 订单状态，如果值为DBNull则设为空字符串
                                Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : string.Empty,
                            };
                        }
                    }
                }
            }
            // 如果未找到订单，返回null
            return null;
        }


        public IActionResult CancelOrder(int orderId)
        {
            // 取消订单
            // TODO: 实现取消订单的逻辑
            return RedirectToAction("Index");
        }

        // GET: Order/ContactDoctor
        public async Task<IActionResult> ContactDoctor(int orderId)
        {
            // 获取订单信息用于联系医生
            var order = await GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/SendMessage
        [HttpPost]
        public IActionResult SendMessage(int orderId, string message)
        {
            // 发送消息给医生
            // TODO: 实现发送消息的逻辑
            return RedirectToAction("Index");
        }

        // GET: Order/EditOrder
        public async Task<IActionResult> EditOrder(int orderId)
        {
            // 获取订单信息用于编辑
            var order = await GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/UpdateOrder
        [HttpPost]
        public IActionResult UpdateOrder(Orders order)
        {
            // 更新订单信息
            // TODO: 实现更新订单的逻辑
            return RedirectToAction("Index");
        }

        // GET: Order/AfterSales
        public async Task<IActionResult> AfterSales(int orderId)
        {
            // 获取订单信息用于售后服务
            var order = await GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/SubmitAfterSales
        [HttpPost]
        public IActionResult SubmitAfterSales(int orderId, string issueType, string description, string contactMethod)
        {
            // 提交售后申请
            // TODO: 实现提交售后申请的逻辑
            return RedirectToAction("Index");
        }
    }
}