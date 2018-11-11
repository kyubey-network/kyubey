using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;
using Pomelo.AspNetCore.Localization;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class BaseController : BaseController<KyubeyContext>
    {
        protected string currentCulture = null;
        public override void Prepare()
        {
            base.Prepare();
            ViewBag.OtcContract = Configuration["Contracts:Otc"];
            ViewBag.DexContract = Configuration["Contracts:Dex"];
            ViewBag.Flex = false;

            var _cultureProvider = (ICultureProvider)HttpContext.RequestServices.GetService(typeof(ICultureProvider));
            currentCulture = _cultureProvider.DetermineCulture();
            ViewBag.Culture = currentCulture;
        }
    }
}
