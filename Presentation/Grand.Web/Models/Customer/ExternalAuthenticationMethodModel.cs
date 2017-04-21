using System.Web.Routing;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Customer
{
    public partial class ExternalAuthenticationMethodModel : BaseGrandModel
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}