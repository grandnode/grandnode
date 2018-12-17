using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Api.Model.Catalog;
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

                builder.EntitySet<Category>(typeof(Category).Name);
                builder.EntityType<Category>().Count().Filter().OrderBy().Expand().Select().Page();

                #endregion
            }
        }
        public int Order => 0;
    }
}
