using Grand.Core.Models;
using Grand.Domain.Messages;

namespace Grand.Web.Models.Messages
{
    public partial class InteractiveFormModel : BaseEntityModel
    {
        public InteractiveForm InteractiveForm { get; set; }
        public string Body { get; set; }
    }
}