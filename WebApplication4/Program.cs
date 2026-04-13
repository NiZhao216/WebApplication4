using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
namespace WebApplication4
{
    /// <summary>
    /// 程序的主入口类
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 程序的主入口方法
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static void Main(string[] args)
        {
            // 创建 WebApplication 构建器
            var builder = WebApplication.CreateBuilder(args);

            // 1. 添加控制器与视图服务（默认已有）
            builder.Services.AddControllersWithViews();
            // 配置全局过滤器，为所有请求添加登录令牌过滤器
            builder.Services.AddControllersWithViews(options =>
            {
                // 👇 这一行就是【全局注册】！所有请求自动拦截
                options.Filters.Add<Logintokenfilterz>();
            });

            // 2. 配置 Cookie 认证（核心！）
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // 未登录时自动跳转的登录页地址
                    options.LoginPath = "/Login/Index";
                    // 登录状态超时时间（20分钟）
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    // 访问时自动续期
                    options.SlidingExpiration = true;
                    // 权限跳转
                    options.AccessDeniedPath = "/Home/Index";

                });

            var app = builder.Build();

            // 3. 中间件顺序：必须在 UseAuthorization 之前
            app.UseStaticFiles();
            app.UseRouting();

            // 启用认证 + 授权
            app.UseAuthentication(); // 👈 必须加这行
            app.UseAuthorization();  // 👈 必须加这行

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
