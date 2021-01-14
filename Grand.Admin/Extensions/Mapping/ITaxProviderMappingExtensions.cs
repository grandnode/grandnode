using Grand.Services.Tax;
using Grand.Admin.Models.Tax;

namespace Grand.Admin.Extensions
{
    public static class ITaxProviderMappingExtensions
    {
        public static TaxProviderModel ToModel(this ITaxProvider entity)
        {
            return entity.MapTo<ITaxProvider, TaxProviderModel>();
        }
    }
}