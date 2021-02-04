using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public partial class HeaderLinksModel : BaseModel
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerEmailUsername { get; set; }
    }
}