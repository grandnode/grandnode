using Grand.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("[area]/[controller]/[action]")]
    public abstract partial class BaseApiController : BaseController
    {
    }
}
