using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Manage.Controllers
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
    }
}
