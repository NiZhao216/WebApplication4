using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySqlConnector;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication4
{
    public class Logintokenfilterz: IAsyncAuthorizationFilter
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 匿名页面跳过
            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute);
            if (hasAllowAnonymous) return;

            // ====================== 核心：自动读取页面角色（不用改网址！） ======================
            var authorizeData = context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().FirstOrDefault();
            // 自动获取当前页面是【医生登录】还是【用户登录】
            string needScheme = authorizeData?.AuthenticationSchemes ?? "UserAuth";
            // ==================================================================================

            // 只校验当前页面需要的身份
            var myIdentity = context.HttpContext.User.Identities
                .FirstOrDefault(i => i.AuthenticationType == needScheme && i.IsAuthenticated);

            if (myIdentity == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // 校验Token
            string username = myIdentity.Name;
            string token = myIdentity.FindFirst("logintoken")?.Value;
            string dbtoken = null;

            using var conn = new MySqlConnection(_conStr);
            await conn.OpenAsync();

            if (needScheme == "DoctorAuth")
            {
                var cmd = new MySqlCommand("SELECT token FROM doctors WHERE did = @username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                dbtoken = (await cmd.ExecuteScalarAsync())?.ToString();
            }
            else
            {
                var cmd = new MySqlCommand("SELECT token FROM userstable WHERE username = @username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                dbtoken = (await cmd.ExecuteScalarAsync())?.ToString();
            }

            if (dbtoken != token && !string.IsNullOrEmpty(token))
            {
                await context.HttpContext.SignOutAsync(needScheme);
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}