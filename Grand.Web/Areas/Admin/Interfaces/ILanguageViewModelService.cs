using Grand.Core.Domain.Localization;
using Grand.Web.Areas.Admin.Models.Localization;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ILanguageViewModelService
    {
        void PrepareFlagsModel(LanguageModel model);
        void PrepareCurrenciesModel(LanguageModel model);
        Language InsertLanguageModel(LanguageModel model);
        Language UpdateLanguageModel(Language language, LanguageModel model);
        (bool error, string message) InsertLanguageResourceModel(LanguageResourceModel model);
        (bool error, string message) UpdateLanguageResourceModel(LanguageResourceModel model);
        (IEnumerable<LanguageResourceModel> languageResourceModels, int totalCount) PrepareLanguageResourceModel(LanguageResourceFilterModel model, string languageId, int pageIndex, int pageSize);
    }
}
