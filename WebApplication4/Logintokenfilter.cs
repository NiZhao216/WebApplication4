using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySqlConnector;
using System.Collections.Specialized;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication4
{
    public class Logintokenfilterz: IAsyncAuthorizationFilter
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";
/// <summary>
/// 异步授权方法，用于验证用户登录状态和令牌的有效性
/// </summary>
/// <param name="context">授权过滤器上下文，包含HTTP请求信息和授权相关数据</param>
/// <returns>一个异步任务，表示授权操作的执行过程</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
    // 检查用户是否已通过身份验证
                 bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

                if (hasAllowAnonymous) return;
              
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {

                using (var conn = new MySqlConnection(_conStr))
                {
            // 异步打开数据库连接
                    await conn.OpenAsync();
            // 定义SQL查询语句，用于获取指定用户的令牌
                    string sql= "SELECT token FROM userstable WHERE username = @username";
            // 创建MySQL命令对象
                    MySqlCommand cmd=new (sql, conn);
            // 添加参数，防止SQL注入攻击
                    cmd.Parameters.AddWithValue("@username", context.HttpContext.User.Identity.Name);
            // 执行查询并获取数据库中的令牌
                    string dbtoken= (await cmd.ExecuteScalarAsync())?.ToString();
            // 比较数据库中的令牌与当前用户令牌是否一致
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
