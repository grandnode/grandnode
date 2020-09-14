using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public partial class LanguageModel : BaseEntityModel
    {
        public string Name { get; set; }

        public string FlagImageFileName { get; set; }

    }
}