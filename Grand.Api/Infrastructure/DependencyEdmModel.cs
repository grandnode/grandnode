using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Api.DTOs;
using Grand.Core.Configuration;
using Microsoft.AspNet.OData.Builder;
using Grand.Api.DTOs.Catalog;

namespace Grand.Api.Infrastructure
{
    public class DependencyEdmModel : IDependencyEdmModel
    {

        public void Register(ODataConventionModelBuilder builder, ApiConfig apiConfig)
        {
            if (apiConfig.SystemModel)
            {
                #region Category model

                builder.EntitySet<CategoryDTO>("Category");
                builder.EntityType<CategoryDTO>().Count().Filter().OrderBy().Page();

                #endregion
            }
        }
        public int Order => 0;
    }
}
