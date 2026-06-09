using Microsoft.AspNetCore.Mvc;

namespace CryptonicsPropertyManagement.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index() => View();
    }
}
