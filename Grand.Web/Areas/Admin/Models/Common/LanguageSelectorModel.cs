using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Localization;
using Grand.Framework.Mvc;

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