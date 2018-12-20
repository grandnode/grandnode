using Grand.Api.DTOs.Catalog;
using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Core.Configuration;
using Microsoft.AspNet.OData.Builder;

namespace Grand.Api.Infrastructure
{
    public class DependencyEdmModel : IDependencyEdmModel
    {

        public void Register(ODataConventionModelBuilder builder, ApiConfig apiConfig)
        {
            if (apiConfig.SystemModel)
            {
                #region Category model

                builder.EntitySet<CategoryDto>("Category");
                builder.EntityType<CategoryDto>().Count().Filter().OrderBy().Page();

                #endregion

                #region Manufacturer model

                builder.EntitySet<ManufacturerDto>("Manufacturer");
                builder.EntityType<ManufacturerDto>().Count().Filter().OrderBy().Page();

                #endregion
            }
        }
        public int Order => 0;
    }
}
