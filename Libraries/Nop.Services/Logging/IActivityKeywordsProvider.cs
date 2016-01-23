using System.Collections.Generic;

namespace Nop.Services.Logging
{
    public partial interface IActivityKeywordsProvider
    {
        IList<string> GetCategorySystemKeywords();
        IList<string> GetProductSystemKeywords();
        IList<string> GetManufacturerSystemKeywords();

    }
}
