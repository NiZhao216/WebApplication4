// Controllers/DoctorsManagement/DoctorsManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using MySqlConnector;

namespace WebApplication4.Controllers.DoctorsManagement
{
    [Authorize(Roles = "管理员")]
    public class DoctorsManagementController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            List<Doctors> doctors = await GetDoctors(pageIndex); // 获取第一页医生数据
            return View(doctors);
        }

        public async Task<List<Doctors>> GetDoctors(int pageIndex)
        {
            // 固定配置
            const int pageSize = 20;
            // 分页偏移量公式
            int offset = (pageIndex - 1) * pageSize;
            List<Doctors> doctorList = new List<Doctors>();

            // using：自动关闭数据库连接（企业强制规范）
            using (MySqlConnection conn = new MySqlConnection(_conStr))
            {
                // SQL：排序 + 分页
                string sql = @"
                    SELECT did, doctor_name, gender, department, title, status
                    FROM doctors
                    ORDER BY did DESC
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
                        using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Doctors doctor = new Doctors();
                                // 安全取值，避免空值报错
                                doctor.DoctorId = reader.GetInt32("did");
                                doctor.DoctorName = reader.GetString("doctor_name") ?? "";
                                doctor.Gender = reader.GetString("gender") ?? "";
                                doctor.Department = reader.GetString("department") ?? "";
                                doctor.Title = reader.GetString("title") ?? "";
                                doctor.Status = reader.GetString("status") ?? "";
                                doctorList.Add(doctor);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 企业级：这里替换为日志记录（如Log4net/NLog）
                        Console.WriteLine($"查询医生分页失败：{ex.Message}");
                    }
                }
            }

            return doctorList;
        }
    }
}