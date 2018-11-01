using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;
using Pomelo.AspNetCore.Localization;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class BaseController : BaseController<KyubeyContext>
    {
        protected readonly ICultureProvider _cultureProvider = null;
        public BaseController(ICultureProvider cultureProvider)
        {
            _cultureProvider = cultureProvider;
        }
        public override void Prepare()
        {
            base.Prepare();
            ViewBag.OtcContract = Configuration["Contracts:Otc"];
            ViewBag.DexContract = Configuration["Contracts:Dex"];
            ViewBag.Flex = false;
        }
    }
}
