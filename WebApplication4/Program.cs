using Microsoft.AspNetCore.Authentication.Cookies;
using WebSocketdemo;
namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 恢复全局过滤器，现在支持双角色
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<Logintokenfilterz>(); 
            });

            // 双Cookie认证（用户+医生，无冲突）
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "UserAuth";
            })
            .AddCookie("UserAuth", options =>
            {
                options.Cookie.Name = ".USER_AUTH";
                
                options.LoginPath = "/Login/Index";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                options.SlidingExpiration = true;
            })
            .AddCookie("DoctorAuth", options =>
            {
                options.Cookie.Name = ".DOCTOR_AUTH";
                
                options.LoginPath = "/Login/Index";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                options.SlidingExpiration = true;
            });

            builder.Services.AddSingleton<WebSocketHub>();
            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseWebSockets();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}