using Grand.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Api.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("[area]/[controller]/[action]")]
    public abstract partial class BaseApiController : Controller
    {
    }
}
