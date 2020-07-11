using Grand.Domain.Messages;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public class PopupModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public string CustomerActionId { get; set; }
        public PopupType PopupType { get; set; }
    }
}
