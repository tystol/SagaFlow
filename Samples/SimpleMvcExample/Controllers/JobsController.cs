using Microsoft.AspNetCore.Mvc;

namespace SimpleMvcExample.Controllers;

public class JobsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}