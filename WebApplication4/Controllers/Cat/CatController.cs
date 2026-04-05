using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;
namespace WebApplication4.Controllers.Cat
{
    public class CatController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        public IActionResult Index()
        {
            Cats cat = GetCats(User.Identity?.Name ?? string.Empty).Result;
            return View(cat);
        }
        [HttpPost]
        public async Task<IActionResult> SaveCat(Cats cat)
        {


            // 正确语法：无多余参数，无WHERE条件，靠唯一键自动判断
            string sql = @"
    INSERT INTO userscat (
        catname, gender, age, breed, weight, birthday, coatcolor, 
        allergy, medicalhistory, vaccinestatus, nextvaccinedate, 
        dewormstatus, nextdewormdate, username
    ) 
    VALUES (
        @catname, @gender, @age, @breed, @weight, @birthday, @coatcolor,
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

            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // 只用一套参数，完美支持 新增+更新
                    cmd.Parameters.AddWithValue("@catname", cat.CatName ?? "");
                    cmd.Parameters.AddWithValue("@gender", cat.Gender ?? "");
                    cmd.Parameters.AddWithValue("@age", cat.Age ?? "");
                    cmd.Parameters.AddWithValue("@breed", cat.Breed ?? "");
                    cmd.Parameters.AddWithValue("@weight", cat.Weight ?? "");
                    cmd.Parameters.AddWithValue("@birthday", (object)cat.Birthday! ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@coatcolor", cat.CoatColor ?? "");
                    cmd.Parameters.AddWithValue("@allergy", cat.Allergy ?? "");
                    cmd.Parameters.AddWithValue("@medicalhistory", cat.MedicalHistory ?? "");
                    cmd.Parameters.AddWithValue("@vaccinestatus", cat.VaccineStatus ?? "");
                    cmd.Parameters.AddWithValue("@nextvaccinedate", (object)cat.NextVaccineDate! ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@dewormstatus", cat.DewormStatus ?? "");
                    cmd.Parameters.AddWithValue("@nextdewormdate", (object)cat.NextDewormDate! ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@username", User.Identity!.Name);

                    int num = await cmd.ExecuteNonQueryAsync();
                    if (num > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }

            return RedirectToAction("Index");
        }
        private async Task<Cats> GetCats(string username)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM userscat WHERE username=@username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
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