using System.Collections.Generic;

namespace Grand.Services.Logging
{
    public partial interface IActivityKeywordsProvider
    {
        IList<string> GetCategorySystemKeywords();
        IList<string> GetProductSystemKeywords();
        IList<string> GetManufacturerSystemKeywords();

    }
}
