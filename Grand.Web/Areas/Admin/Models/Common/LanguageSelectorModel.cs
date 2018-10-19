using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Localization;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class LanguageSelectorModel : BaseGrandModel
    {
        public LanguageSelectorModel()
        {
            AvailableLanguages = new List<LanguageModel>();
        }

        public IList<LanguageModel> AvailableLanguages { get; set; }

        public LanguageModel CurrentLanguage { get; set; }
    }
}