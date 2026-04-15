using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication4.Models;
using webmsg;
using WebSocketdemo;
using Websocket.Controllers;
namespace WebApplication4.Controllers.Yuy
{
 
   
   [Authorize]
    public class YuyController : Controller
    {
       
        public  WebSocketHub _hub;

        public YuyController(WebSocketHub s)
        {
            _hub = s;
        }
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        
        public async Task<IActionResult> Index()
        {
            List<Cats> cats = await Getcats();
            List<Doctors> doctors=await Getdoctors();
            return View((cats,doctors));
        }
        public async Task<IActionResult> Createorder(Orders od)
        {
            using(var con = new MySqlConnection(_conStr))
            {
                await con.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText =@"INSERT INTO orders (username, catid, catname, phone, servicetype, appointmenttime, description, did, doctor_name)  VALUES (@UserName, @CatId, @CatName, @Phone, @ServiceType, @AppointmentTime,@Description, @DoctorId, @DoctorName)";
                    cmd.Parameters.AddWithValue("@UserName", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@CatId", od.CatId);
                    cmd.Parameters.AddWithValue("@CatName", od.CatName);
                    cmd.Parameters.AddWithValue("@Phone", od.Phone);
                    cmd.Parameters.AddWithValue("@ServiceType", od.ServiceType);
                    cmd.Parameters.AddWithValue("@AppointmentTime", od.AppointmentTime);
                    cmd.Parameters.AddWithValue("@Description", od.Description);
                    cmd.Parameters.AddWithValue("@DoctorId", od.DoctorId);
                    cmd.Parameters.AddWithValue("@DoctorName", od.DoctorName);
                    if(await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        
                        await WebsocketController.HandleOrderQuery(od.DoctorName, "医生");
                        return RedirectToAction("Index","Order");
                    }
                    else
                    {
                        TempData["Message"] = "预约失败，请重试";
                        return RedirectToAction("Index");
                    }
                }
            }
        }
        /// <summary>
        /// 异步获取当前用户的猫咪信息
        /// </summary>
        /// <returns>返回包含猫咪信息的列表，如果没有找到则返回null</returns>
        public async Task<List<Cats>> Getcats()
        {
            // 创建一个猫咪信息列表
            List<Cats> list = new List<Cats>();
            // 使用using语句创建数据库连接，确保连接在使用后被正确关闭
            using (var con = new MySqlConnection(_conStr))
            {
                // 异步打开数据库连接
                await con.OpenAsync();
                // 使用using语句创建命令对象，确保命令在使用后被正确释放
                using (var cmd = new MySqlCommand())
                {
                    // 设置命令的连接和SQL查询文本
                    cmd.Connection = con;
                    cmd.CommandText = "select * from userscat where username=@username";
                    // 添加参数，防止SQL注入
                    cmd.Parameters.AddWithValue("@username", User.Identity.Name);
                    // 使用using语句创建数据读取器，确保读取器在使用后被正确关闭
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 异步读取数据，如果读取到数据
                        while( await reader.ReadAsync())
                        {
                            // 将读取到的数据添加到列表中
                            list.Add(new Cats()  
                            { CatId = reader["catid"] != DBNull.Value ? Convert.ToInt32(reader["catid"]) : 0,
                        CatName = reader["catname"] != DBNull.Value ? reader["catname"].ToString() : string.Empty,
                        Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : string.Empty,
                        Age = reader["age"] != DBNull.Value ? reader["age"].ToString() : string.Empty,
                        Breed = reader["breed"] != DBNull.Value ? reader["breed"].ToString() : string.Empty,
                        Weight = reader["weight"] != DBNull.Value ? reader["weight"].ToString() : string.Empty,
                        Birthday = reader["birthday"] != DBNull.Value ? Convert.ToDateTime(reader["birthday"]) : (DateTime?)null,
                        CoatColor = reader["coatcolor"] != DBNull.Value ? reader["coatcolor"].ToString() : string.Empty,
                        Allergy = reader["allergy"] != DBNull.Value ? reader["allergy"].ToString() : string.Empty,
                        MedicalHistory = reader["medicalhistory"] != DBNull.Value ? reader["medicalhistory"].ToString() : string.Empty,
                        VaccineStatus = reader["vaccinestatus"] != DBNull.Value ? reader["vaccinestatus"].ToString() : string.Empty,
                        NextVaccineDate = reader["nextvaccinedate"] != DBNull.Value ? Convert.ToDateTime(reader["nextvaccinedate"]) : (DateTime?)null,
                        DewormStatus = reader["dewormstatus"] != DBNull.Value ? reader["dewormstatus"].ToString() : string.Empty,
                        NextDewormDate = reader["nextdewormdate"] != DBNull.Value ? Convert.ToDateTime(reader["nextdewormdate"]) : (DateTime?)null,
                        Username = reader["username"] != DBNull.Value ? reader["username"].ToString() : string.Empty,
                        avatar = reader["avatar"] != DBNull.Value ? reader["avatar"].ToString() : string.Empty
                      });
                           
                           
                        }
                    
                        if(list.Count>0)
                            return list;
                        else
                            return null;
                        
                    }
                }
        
            }   
        }
        public async Task<List<Doctors>> Getdoctors()
        {

            List<Doctors> list = new List<Doctors>();
            using(var con = new MySqlConnection(_conStr))
            {
                
                await con.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "select * from doctors";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Doctors()
                            {
                                DoctorId = reader["did"] != DBNull.Value ? Convert.ToInt32(reader["did"]) : 0,
                                DoctorName = reader["doctor_name"] != DBNull.Value ? reader["doctor_name"].ToString() : string.Empty,
                                Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : string.Empty,
                                Department = reader["department"] != DBNull.Value ? reader["department"].ToString() : string.Empty,
                                Title= reader["title"] != DBNull.Value ? reader["title"].ToString() : string.Empty,
                                LicenseNo= reader["license_no"] != DBNull.Value ? reader["license_no"].ToString() : string.Empty,
                                Avatar= reader["avatar"] != DBNull.Value ? reader["avatar"].ToString() : string.Empty,
                                Introduction= reader["introduction"] != DBNull.Value ? reader["introduction"].ToString() : string.Empty,
                                Specialty= reader["specialty"] != DBNull.Value ? reader["specialty"].ToString() : string.Empty,
                                Motto= reader["motto"] != DBNull.Value ? reader["motto"].ToString() : string.Empty,
                                Phone= reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty,
                                HireDate= reader["hire_date"] != DBNull.Value ? Convert.ToDateTime(reader["hire_date"]) : (DateTime?)null,
                                Status= reader["status"] != DBNull.Value ? reader["status"].ToString() : string.Empty,
                                CreateTime= reader["create_time"] as DateTime?,
                                UpdateTime= reader["update_time"] as DateTime?
                            });
                        }
                        if(list.Count > 0)
                            return list;
                            else
                            return null;
                    }
                    }
            }
        }
    }
}
