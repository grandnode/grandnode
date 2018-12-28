using Grand.Api.DTOs.Catalog;
using Grand.Api.DTOs.Common;
using Grand.Api.DTOs.Customers;
using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Core.Configuration;
using Microsoft.AspNet.OData.Builder;

namespace Grand.Api.Infrastructure
{
    public class DependencyEdmModel : IDependencyEdmModel
    {
        protected void RegisterCommon(ODataConventionModelBuilder builder)
        {
            #region Language model

            builder.EntitySet<LanguageDto>("Language");
            builder.EntityType<LanguageDto>().Count().Filter().OrderBy().Page();

            #endregion
        }

        protected void RegisterCatalog(ODataConventionModelBuilder builder)
        {
            #region Category model

            builder.EntitySet<CategoryDto>("Category");
            builder.EntityType<CategoryDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Manufacturer model

            builder.EntitySet<ManufacturerDto>("Manufacturer");
            builder.EntityType<ManufacturerDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Product attribute model

            builder.EntitySet<ProductAttributeDto>("ProductAttribute");
            builder.EntityType<ProductAttributeDto>().Count().Filter().OrderBy().Page();
            builder.ComplexType<PredefinedProductAttributeValueDto>();

            #endregion

            #region Product attribute model

            builder.EntitySet<SpecificationAttributeDto>("SpecificationAttribute");
            builder.EntityType<SpecificationAttributeDto>().Count().Filter().OrderBy().Page();
            builder.ComplexType<SpecificationAttributeOptionDto>();

            #endregion

        }

        protected void RegisterCustomers(ODataConventionModelBuilder builder)
        {
            #region Customer

            builder.EntitySet<CustomerDto>("Customer");
            var customer = builder.EntityType<CustomerDto>();
            builder.ComplexType<AddressDto>();

            ActionConfiguration addAddress = customer.Action("AddAddress");
            addAddress.Parameter<AddressDto>("address");
            addAddress.Returns<AddressDto>();

            ActionConfiguration updateAddress = customer.Action("UpdateAddress");
            updateAddress.Parameter<AddressDto>("address");
            updateAddress.Returns<AddressDto>();

            ActionConfiguration deleteAddress = customer.Action("DeleteAddress");
            deleteAddress.Parameter<string>("addressId");
            deleteAddress.Returns<bool>();

            //api/Customer(email)/ChangePassword - body contains text with password
            ActionConfiguration changePassword = customer.Action("SetPassword");
            changePassword.Parameter<string>("password");
            changePassword.Returns<bool>();

            #endregion

            #region Customer Role model

            builder.EntitySet<CustomerRoleDto>("CustomerRole");
            builder.EntityType<CustomerRoleDto>().Count().Filter().OrderBy().Page();

            #endregion
        }

        public void Register(ODataConventionModelBuilder builder, ApiConfig apiConfig)
        {
            if (apiConfig.SystemModel)
            {
                RegisterCommon(builder);
                RegisterCatalog(builder);
                RegisterCustomers(builder);
            }
        }

        public int Order => 0;
    }
}
