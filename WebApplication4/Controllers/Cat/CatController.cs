using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;
namespace WebApplication4.Controllers.Cat
{
    public class CatController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        
        public IActionResult Index(string catName)
        {
            Cats cat = GetCats(User.Identity?.Name ?? string.Empty, catName).Result;
            return View(cat);
        }
        public IActionResult InCreatedex()
        {
            return View("~/Views/Cat/Index.cshtml",new Cats());
        }

        [HttpPost]
        public async Task<IActionResult> SaveCat(Cats cat)
        {
           

            int id = 0;

            // 2. 【优化】仅创建一次数据库连接（查询+增改共用，提升性能）
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();

                // 第一步：查询猫咪旧名称对应的ID
                string sqlid = "select id from userscat where username=@username and catname=@catname";
                using (MySqlCommand cmd = new MySqlCommand(sqlid, conn))
                {
                    cmd.Parameters.AddWithValue("@username", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@catname", cat.oldname ?? ""); // 空值兼容
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        id = Convert.ToInt32(result);
                    }
                }

                // 第二步：新增/更新数据（ON DUPLICATE KEY 语法）
                string sql = @"
            INSERT INTO userscat (
                id, catname, gender, age, breed, weight, birthday, coatcolor, 
                allergy, medicalhistory, vaccinestatus, nextvaccinedate, 
                dewormstatus, nextdewormdate, username
            ) 
            VALUES (
                @id, @catname, @gender, @age, @breed, @weight, @birthday, @coatcolor,
                @allergy, @medicalhistory, @vaccinestatus, @nextvaccinedate,
                @dewormstatus, @nextdewormdate, @username
            )
            ON DUPLICATE KEY UPDATE 
                catname = VALUES(catname),
                gender = VALUES(gender),
                age = VALUES(age),
                breed = VALUES(breed),
                weight = VALUES(weight),
                birthday = VALUES(birthday),
                coatcolor = VALUES(coatcolor),
                allergy = VALUES(allergy),
                medicalhistory = VALUES(medicalhistory),
                vaccinestatus = VALUES(vaccinestatus),
                nextvaccinedate = VALUES(nextvaccinedate),
                dewormstatus = VALUES(dewormstatus),
                nextdewormdate = VALUES(nextdewormdate);
        ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 【核心修复】添加缺失的 @id 参数！原代码没有这个参数会直接报错
                    cmd.Parameters.AddWithValue("@id", id);

                    // 统一空值处理：字符串空值传空，日期空值传DBNull
                    cmd.Parameters.AddWithValue("@catname", cat.CatName ?? "");
                    cmd.Parameters.AddWithValue("@gender", cat.Gender ?? "");
                    cmd.Parameters.AddWithValue("@age", cat.Age ?? "");
                    cmd.Parameters.AddWithValue("@breed", cat.Breed ?? "");
                    cmd.Parameters.AddWithValue("@weight", cat.Weight ?? "");
                    cmd.Parameters.AddWithValue("@birthday", cat.Birthday ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@coatcolor", cat.CoatColor ?? "");
                    cmd.Parameters.AddWithValue("@allergy", cat.Allergy ?? "");
                    cmd.Parameters.AddWithValue("@medicalhistory", cat.MedicalHistory ?? "");
                    cmd.Parameters.AddWithValue("@vaccinestatus", cat.VaccineStatus ?? "");
                    cmd.Parameters.AddWithValue("@nextvaccinedate", cat.NextVaccineDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@dewormstatus", cat.DewormStatus ?? "");
                    cmd.Parameters.AddWithValue("@nextdewormdate", cat.NextDewormDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@username", User.Identity.Name);

                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    if (affectedRows > 0)
                    {
                        return RedirectToAction("Index", "Catlist");
                    }
                }
            }

            return RedirectToAction("Index");
        }
        private async Task<Cats> GetCats(string username,string catName)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM userscat WHERE username=@username and catname=@usercat";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@usercat", catName);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Cats
                            {
                                CatName = reader["catname"]?.ToString() ?? "",
                                Gender = reader["gender"]?.ToString() ?? "",
                                Age = reader["age"]?.ToString() ?? "",
                                Breed = reader["breed"]?.ToString() ?? "",
                                Weight = reader["weight"]?.ToString() ?? "",
                                Birthday = reader["birthday"] as DateTime?,
                                CoatColor = reader["coatcolor"]?.ToString() ?? "",
                                Allergy = reader["allergy"]?.ToString() ?? "",
                                MedicalHistory = reader["medicalhistory"]?.ToString() ?? "",
                                VaccineStatus = reader["vaccinestatus"]?.ToString() ?? "",
                                NextDewormDate = reader["nextdewormdate"] as DateTime?,
                                DewormStatus = reader["dewormstatus"]?.ToString() ?? "",
                                NextVaccineDate = reader["nextvaccinedate"] as DateTime?
                            };

                        }
                        else
                        {
                            return new Cats();
                        }
                    }

                }
            }
        }
    }
}