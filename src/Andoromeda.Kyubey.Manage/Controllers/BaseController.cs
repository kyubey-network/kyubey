using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class BaseController : BaseController<KyubeyContext, User, long>
    {
    }
}
