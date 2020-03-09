using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Grand.Framework.Mvc.Routing
{
    public class LanguageParameterTransformer : IOutboundParameterTransformer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageParameterTransformer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string TransformOutbound(object value)
        {
            var lang = _httpContextAccessor.HttpContext.Request.RouteValues["language"];
            if (lang != null)
                return lang.ToString();

            return value == null ? null : value.ToString();
        }
    }
}
