// Controllers/Admin/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using MySqlConnector;
namespace WebApplication4.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        public IActionResult Index()
        {
            return View();
        }
       
    }
}
