using MySqlConnector;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication4.Controllers.DoctorHome
{
    [Authorize(AuthenticationSchemes = "DoctorAuth", Roles = "医生")]
    public class DoctorHomeController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Doctor/Index.cshtml");
        }
    }
    
}
