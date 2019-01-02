using Grand.Api.DTOs.Catalog;
using Grand.Api.DTOs.Common;
using Grand.Api.DTOs.Customers;
using Grand.Api.DTOs.Shipping;
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

            #region Currency model

            builder.EntitySet<CurrencyDto>("Currency");
            builder.EntityType<CurrencyDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Store model

            builder.EntitySet<StoreDto>("Store");
            builder.EntityType<StoreDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Country model

            builder.EntitySet<CountryDto>("Country");
            builder.EntityType<CountryDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region State province model

            builder.EntitySet<StateProvinceDto>("StateProvince");
            builder.EntityType<StateProvinceDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Templates model

            builder.EntitySet<MessageTemplateDto>("CategoryTemplate");
            builder.EntitySet<MessageTemplateDto>("ManufacturerTemplate");
            builder.EntitySet<MessageTemplateDto>("ProductTemplate");
            builder.EntityType<MessageTemplateDto>().Count().Filter().OrderBy().Page();

            #endregion
        }

        protected void RegisterCatalog(ODataConventionModelBuilder builder)
        {
            #region Product model

            builder.EntitySet<ProductDto>("Product");
            var product = builder.EntityType<ProductDto>();
            product.Count().Filter().OrderBy().Page();
            builder.ComplexType<ProductCategoryDto>();
            builder.ComplexType<ProductManufacturerDto>();
            builder.ComplexType<ProductPictureDto>();
            builder.ComplexType<ProductSpecificationAttributeDto>();
            builder.ComplexType<ProductTierPriceDto>();
            builder.ComplexType<ProductWarehouseInventoryDto>();
            builder.ComplexType<ProductAttributeMappingDto>();
            builder.ComplexType<ProductAttributeValueDto>();
            builder.ComplexType<ProductAttributeCombinationDto>();

            //update stock for product
            ActionConfiguration updateStock = product.Action("UpdateStock");
            updateStock.Parameter<string>("WarehouseId");
            updateStock.Parameter<int>("Stock").Required();
            updateStock.Returns<bool>();

            #endregion

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

            ActionConfiguration changePassword = customer.Action("SetPassword");
            changePassword.Parameter<string>("password");
            changePassword.Returns<bool>();

            #endregion

            #region Customer Role model

            builder.EntitySet<CustomerRoleDto>("CustomerRole");
            builder.EntityType<CustomerRoleDto>().Count().Filter().OrderBy().Page();

            #endregion
        }

        protected void RegisterShipping(ODataConventionModelBuilder builder)
        {
            #region Warehouse model

            builder.EntitySet<WarehouseDto>("Warehouse");
            builder.EntityType<WarehouseDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Delivery date model

            builder.EntitySet<DeliveryDateDto>("DeliveryDate");
            builder.EntityType<DeliveryDateDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Pickup point model

            builder.EntitySet<PickupPointDto>("PickupPoint");
            builder.EntityType<PickupPointDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Shipping method model

            builder.EntitySet<ShippingMethodDto>("ShippingMethod");
            builder.EntityType<ShippingMethodDto>().Count().Filter().OrderBy().Page();

            #endregion
        }

        public void Register(ODataConventionModelBuilder builder, ApiConfig apiConfig)
        {
            if (apiConfig.SystemModel)
            {
                RegisterCommon(builder);
                RegisterCatalog(builder);
                RegisterCustomers(builder);
                RegisterShipping(builder);
            }
        }

        public int Order => 0;
    }
}
