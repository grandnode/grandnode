using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class LanguageModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string FlagImageFileName { get; set; }

    }
}