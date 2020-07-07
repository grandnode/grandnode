using Grand.Api.Commands.Models.Common;
using Grand.Api.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Api.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("[area]/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class TokenController : Controller
    {
        private readonly IMediator _mediator;

        public TokenController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginModel model)
        {
            var claims = new Dictionary<string, string>();
            claims.Add("Email", model.Email);

            var token = await _mediator.Send(new GenerateTokenCommand() { Claims = claims });
            return Content(token);
        }
    }
}
