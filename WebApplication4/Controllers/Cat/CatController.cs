using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication4.Models;
using System.IO;

namespace WebApplication4.Controllers.Cat
{
    /// <summary>
    /// 猫咪控制器，处理猫咪相关的页面展示和数据操作
    /// </summary>
    public class CatController : Controller
    {
        /// <summary>
        /// Web主机环境服务，用于获取网站根目录路径
        /// </summary>
        // 1. 修正字段名：_ebHost → _webHost（与参数名统一，避免笔误）
        private readonly IWebHostEnvironment _webHost;

        // 2. 修正构造函数：private → public（关键！让DI容器能创建实例）
        public CatController(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        // 数据库连接字符串，用于连接MySQL数据库
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        #region 页面统一入口（新增/编辑都走这个）
        // 列表跳转详情/编辑
        public async Task<IActionResult> Index(string catName)
        {
            // 根据猫名查询数据（编辑回填）
            Cats cat =await GetCatByName(User.Identity?.Name ?? string.Empty, catName);
            return View(cat);
        }

        // 新增空白页面
        public IActionResult InCreatedex()
        {
            // 新增：Id 默认 0
            return View("~/Views/Cat/Index.cshtml", new Cats());
        }
        #endregion

        #region 【核心】统一提交方法（自动区分新增/编辑）
        [HttpPost]
        public async Task<IActionResult> SaveCat(Cats cat, IFormFile avatar, string oldavatar)
        {
            string username = User.Identity?.Name ?? string.Empty;
            string filePath = string.Empty; // 默认头像路径
            if (avatar != null && avatar.Length > 0) // 修正：Length >=0 → Length >0（避免空文件）
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images", "avatar");
                // 确保文件夹存在（新增：避免上传时文件夹不存在报错）
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatar.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (FileStream s = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(s);
                }
                filePath = "/images/catavatar/" + Path.GetFileName(filePath); // 存储相对路径
            }
            else
            {
                filePath = oldavatar; // 保持原头像不变
            }

            if (string.IsNullOrWhiteSpace(cat.CatName))
            {
                TempData["ERR"] = "猫咪名称不能为空，请输入后再保存！";
                // 跳转回对应页面
                return cat.Id > 0
                    ? RedirectToAction("Index", new { catName = cat.CatName })
                    : RedirectToAction("InCreatedex");
            }

            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();

                // ====================== 1. 重名校验（通用）======================
                bool exists = await CheckCatNameExists(conn, cat.Id, username, cat.CatName);
                if (exists)
                {
                    TempData["ERR"] = "猫咪名称已存在，请重新输入！";
                    return cat.Id > 0
                        ? RedirectToAction("Index", new { catName = cat.CatName })
                        : RedirectToAction("InCreatedex");
                }

                // ====================== 2. 分支执行：新增 OR 编辑 ======================
                if (cat.Id == 0)
                {
                    string insertSql = @"
                        INSERT INTO userscat (
                            catname, gender, age, breed, weight, birthday, coatcolor, 
                            allergy, medicalhistory, vaccinestatus, nextvaccinedate, 
                            dewormstatus, nextdewormdate, username,avatar
                        ) 
                        VALUES (
                            @catname, @gender, @age, @breed, @weight, @birthday, @coatcolor,
                            @allergy, @medicalhistory, @vaccinestatus, @nextvaccinedate,
                            @dewormstatus, @nextdewormdate, @username,@avatar
                        );";

                    using (var cmd = new MySqlCommand(insertSql, conn))
                    {
                        AddParameters(cmd, cat, username, filePath);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    TempData["Msg"] = "新增猫咪成功！";
                }
                else
                {
                    string updateSql = @"
                        UPDATE userscat SET
                            catname = @catname,
                            gender = @gender,
                            age = @age,
                            breed = @breed,
                            weight = @weight,
                            birthday = @birthday,
                            coatcolor = @coatcolor,
                            allergy = @allergy,
                            medicalhistory = @medicalhistory,
                            vaccinestatus = @vaccinestatus,
                            nextvaccinedate = @nextvaccinedate,
                            dewormstatus = @dewormstatus,
                            nextdewormdate = @nextdewormdate,
                            avatar=@avatar
                        WHERE id = @id;";

                    using (var cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", cat.Id); // 主键Id
                        AddParameters(cmd, cat, username, filePath);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    TempData["Msg"] = "编辑猫咪成功！";
                }
            }

            // 保存成功跳转到猫咪列表
            return RedirectToAction("Index", "Catlist");
        }
        #endregion

        #region 工具方法（复用）
        /// <summary>
        /// 根据猫名查询（编辑回填用）
        /// </summary>
        private async Task<Cats> GetCatByName(string username, string catName)
        {
            using (var conn = new MySqlConnection(_conStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM userscat WHERE username=@username AND catname=@catname";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@catname", catName);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Cats
                            {
                                Id = reader.GetInt32("id"),
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
                                NextVaccineDate = reader["nextvaccinedate"] as DateTime?,
                                DewormStatus = reader["dewormstatus"]?.ToString() ?? "",
                                NextDewormDate = reader["nextdewormdate"] as DateTime?,
                                avatar = reader["avatar"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }
            return new Cats();
        }

        /// <summary>
        /// 校验猫咪重名
        /// </summary>
        private async Task<bool> CheckCatNameExists(MySqlConnection conn, int catId, string username, string catName)
        {
            string sql = catId == 0
                ? "SELECT COUNT(*) FROM userscat WHERE username=@username AND catname=@catname"
                : "SELECT COUNT(*) FROM userscat WHERE username=@username AND catname=@catname AND id!=@id";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@catname", catName);
                if (catId > 0) cmd.Parameters.AddWithValue("@id", catId);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        /// <summary>
        /// 统一参数封装
        /// </summary>
        private void AddParameters(MySqlCommand cmd, Cats cat, string username, string avatar)
        {
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
            cmd.Parameters.AddWithValue("@username", username);
            // 3. 修正：@avatar, → @avatar（去掉多余的逗号）
            cmd.Parameters.AddWithValue("@avatar", avatar);
        }
        #endregion
    }
}