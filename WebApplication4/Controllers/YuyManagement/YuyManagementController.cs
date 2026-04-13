using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApplication4.Models;
using MySqlConnector;


namespace WebApplication4.Controllers.YuyManagement
{
    /// <summary>
    /// YuyManagementController 是一个控制器类，负责处理与 Yuy 管理相关的请求。
    /// </summary>
    public class YuyManagementController : Controller
    {
        /// <summary>
        /// Index 方法是默认的动作方法，返回 Yuy 管理的视图。
        /// </summary>
        /// <returns>返回 Yuy 管理的视图</returns>
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            List<Orders> orders = await Getorder(pageIndex); // 获取预约列表
            return View(orders);
        }
        public IActionResult YuyF5(int pageIndex)
        {
            return RedirectToAction("Index");
        }
        public async Task<List<Orders>> Getorder(int pageIndex)
        {
            const int pageSize = 20;
            // 分页偏移量公式
            int offset = (pageIndex - 1) * pageSize;
            List<Orders> orders = new List<Orders>();
            using (MySqlConnection conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM orders LIMIT @Offset, @PageSize";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {

                        Orders order = new Orders
                        {
                            Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
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
                        };
                        orders.Add(order);
                    }      
                }
            }
            return orders;
        }
    }
}