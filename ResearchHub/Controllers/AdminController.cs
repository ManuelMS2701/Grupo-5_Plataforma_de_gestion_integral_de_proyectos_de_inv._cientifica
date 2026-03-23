using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
