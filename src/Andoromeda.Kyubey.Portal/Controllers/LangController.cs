using Microsoft.AspNetCore.Mvc;
using Pomelo.AspNetCore.Localization;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class LangController : Controller
    {
        [Route("[controller]/{id}")]
        public IActionResult Index(string id)
        {
            Response.Cookies.Append("ASPNET_LANG", id);
            return Redirect(string.IsNullOrWhiteSpace(Request.Headers["Referer"].ToString())
                ? "/"
                : Request.Headers["Referer"].ToString());
        }
        [Route("[controller]/Current")]
        public IActionResult Current([FromServices] ICultureProvider _cultureProvider)
        {
            return Content(_cultureProvider.DetermineCulture());
        }
    }
}
