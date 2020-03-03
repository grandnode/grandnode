using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Models.Tax;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ITaxProviderMappingExtensions
    {
        public static TaxProviderModel ToModel(this ITaxProvider entity)
        {
            return entity.MapTo<ITaxProvider, TaxProviderModel>();
        }
    }
}