using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication4.Models;

namespace WebApplication4.Controllers.UserCenter
{
   


    [Authorize]

    public class UserCenterController : Controller
    { 
        
        private readonly IWebHostEnvironment _webHost;
        public UserCenterController(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            User currentUser = await  GetUserInfoByUsername(User.Identity?.Name ?? string.Empty);


            return View(currentUser);
        }
        private readonly string constr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, string username, string email, string phone, string age, string sex, DateTime? by, string address,string pr,IFormFile avatar,string oldAvatar)
        {
            // Basic server-side validation (can be expanded)
            if (string.IsNullOrWhiteSpace(username))
            {

                TempData["Message"] = "用户名不能为空";
                return RedirectToAction("Index");
            }

            using (var conn = new MySqlConnection(constr))
            {
                await conn.OpenAsync();
                // Update user information
                string sql = @"
UPDATE userstable 
SET username = @username, 
    email = @email, 
    phone = @phone, 
    age = @age,
    sex = @sex,
    bry = @by,
    address = @address,
    pr = @pr, 
    avatar=@avatar
WHERE 
    username = @name 
    AND 
    NOT EXISTS (
        SELECT 1 FROM (
            SELECT username FROM userstable WHERE username = @username AND id != @id
        ) AS temp
    )
";
                MySqlCommand cmd=new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", User.Identity?.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@by", by);
                cmd.Parameters.AddWithValue("@sex", sex);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.Parameters.AddWithValue("@pr", pr);
                cmd.Parameters.AddWithValue("@id", id);
                string filePath = string.Empty; // 默认头像路径
                if (avatar != null && avatar.Length >= 0)
                {
                    string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images", "avatar");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatar.FileName;
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (FileStream s = new FileStream(filePath, FileMode.Create))
                    {
                        await avatar.CopyToAsync(s);
                    }
                    filePath="/images/avatar/" + Path.GetFileName(filePath); // 存储相对路径
                }
                else
                {
                    filePath = oldAvatar; // 保持原头像不变
                    
                   
                }
               
                cmd.Parameters.AddWithValue("@avatar", filePath); 
                int num= await cmd.ExecuteNonQueryAsync();
                if (num > 0)
                {
                    if (User.Identity!.Name != username)
                    {
                        // 1. 取出原来的权限角色
                        string role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!;

                        // 2. 只用 用户名 + 权限 重新生成（干净！）
                        var newClaims = new List<Claim>
                         {
                              new Claim(ClaimTypes.Name, username),
                              new Claim(ClaimTypes.Role, role), // 权限还在！
                              new Claim("logintoken", User.FindFirstValue("logintoken")!)
                          };

                        // 3. 重新登录
                        var identity = new ClaimsIdentity(newClaims, User.Identity.AuthenticationType);
                        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                    }
                    TempData["Message"] = "保存成功";
                    return RedirectToAction("Index");
                }
                    
                
                else
                {
                    TempData["Message"] = "修改失败,用户名重复";
                    return RedirectToAction("Index");
                }

                
            }
        }
        private async Task<User> GetUserInfoByUsername(string username)
        {
            using (var conn = new MySqlConnection(constr))
            {
                await conn.OpenAsync();
                // 修正：将不规范的"by"改为birthday（需同步修改数据库字段名）
                string sql = "SELECT * FROM userstable WHERE username=@username";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                username = reader["username"]?.ToString() ?? string.Empty,
                                email = reader["email"]?.ToString() ?? string.Empty,
                                phone = reader["phone"]?.ToString() ?? string.Empty,
                                pr = reader["pr"]?.ToString() ?? string.Empty,
                                age = reader["age"]?.ToString() ?? string.Empty,
                                sex = reader["sex"]?.ToString() ?? string.Empty,
                                by = reader["bry"] as DateTime?,
                                id = (int)reader["id"],
                                address= reader["address"]?.ToString() ?? string.Empty,
                                avatar= reader["avatar"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null!;
        }
    }
}
