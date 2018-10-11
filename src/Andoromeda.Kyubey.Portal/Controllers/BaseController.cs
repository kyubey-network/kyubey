using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class BaseController : BaseController<KyubeyContext>
    {
        public override void Prepare()
        {
            base.Prepare();
            ViewBag.OtcContract = Configuration["Contracts:Otc"];
            ViewBag.DexContract = Configuration["Contracts:Dex"];
            ViewBag.Flex = false;
        }
    }
}
