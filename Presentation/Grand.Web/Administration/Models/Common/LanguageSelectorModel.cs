using System.Collections.Generic;
using Grand.Admin.Models.Localization;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
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