using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Analystick.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        [Authorize]
        public IActionResult Index()
        {
            var claims = User.Claims.Select(c => new {type = c.Type, value = c.Value}).OrderBy(c => c.type);
            var model = (object) JsonConvert.SerializeObject(claims, Formatting.Indented);
            return View(model);
        }
    }
}
