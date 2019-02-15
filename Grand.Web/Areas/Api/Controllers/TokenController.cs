using Grand.Api.Interfaces;
using Grand.Web.Areas.Api.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers
{
    public class TokenController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginModel model)
        {
            var claims = new Dictionary<string, string>();
            claims.Add("Email", model.Email);

            var token = await _tokenService.GenerateToken(claims);
            return Content(token);
        }
    }
}
