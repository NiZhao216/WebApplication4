using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySqlConnector;
using System.Collections.Specialized;
using System.Security.Claims;
namespace Logintokenfilter
{
    public class Logintokenfilterz: IAsyncAuthorizationFilter
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                using (var conn = new MySqlConnection(_conStr))
                {
                    await conn.OpenAsync();
                    string sql= "SELECT token FROM userstable WHERE username = @username";
                    MySqlCommand cmd=new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", context.HttpContext.User.Identity.Name);
                    string dbtoken= (await cmd.ExecuteScalarAsync())?.ToString();
                    if (dbtoken != context.HttpContext.User.FindFirstValue("logintoken"))
                    {
                        await context.HttpContext.SignOutAsync("Cookies");
                        context.Result = new RedirectResult("~/Login/Index");
                    }

                }
            }
        }
    }
}   
