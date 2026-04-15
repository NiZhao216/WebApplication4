using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication4.Controllers.Doctor
{
    [Authorize(AuthenticationSchemes = "DoctorAuth", Roles = "医生")]
    public class AppointmentsController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        // GET: Doctor/AppointmentManagement
        public async Task<IActionResult> Index()
        {
            // 从数据库获取医生的预约列表
            List<Orders> orders = await GetAppointments();
            return View("~/Views/Doctor/AppointmentManagement/Index.cshtml", orders);
        }

        // GET: Doctor/AppointmentManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
          
           
            return View();
        }

        // GET: Doctor/AppointmentManagement/Confirm/5
        public IActionResult Confirm(int id)
        {
            // TODO: 确认预约
            return RedirectToAction("Index");
        }

        // GET: Doctor/AppointmentManagement/Complete/5
        public IActionResult Complete(int id)
        {
            // TODO: 完成预约
            return RedirectToAction("Index");
        }

        // GET: Doctor/AppointmentManagement/Cancel/5
        public IActionResult Cancel(int id)
        {
            // TODO: 取消预约
            return RedirectToAction("Index");
        }
        public async Task<List<Orders>> GetAppointments()
        {
            using (MySqlConnection connection = new MySqlConnection(_conStr))
            {
                await connection.OpenAsync();
                string sql = "SELECT * FROM orders WHERE did = @DoctorName";
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@DoctorName", User.Identity?.Name ?? "");
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        List<Orders> ordersList = new List<Orders>();
                        while (await reader.ReadAsync())
                        {
                            Orders order = new Orders
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
                                CreateTime = reader["createtime"] != DBNull.Value ? Convert.ToDateTime(reader["createtime"]) : DateTime.MinValue
                            };
                            ordersList.Add(order);
                        }
                        return ordersList;
                    }
                }
            
            }
            
        }
    }
}
