using Grand.Domain.Localization;
using Grand.Admin.Models.Localization;

namespace Grand.Admin.Extensions
{
    public static class LanguageMappingExtensions
    {
        public static LanguageModel ToModel(this Language entity)
        {
            return entity.MapTo<Language, LanguageModel>();
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return model.MapTo<LanguageModel, Language>();
        }

        public static Language ToEntity(this LanguageModel model, Language destination)
        {
            return model.MapTo(destination);
        }
    }
}