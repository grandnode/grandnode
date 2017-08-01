using Microsoft.AspNetCore.Mvc;

namespace Grand.Framework.Mvc
{
    public class NullJsonResult : JsonResult
    {
        public NullJsonResult() : base(null)
        {
        }
    }
}
