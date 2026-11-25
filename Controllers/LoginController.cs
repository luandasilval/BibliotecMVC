using Microsoft.AspNetCore.Mvc;

namespace BibliotecMVC.Controllers;

public class LoginController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}