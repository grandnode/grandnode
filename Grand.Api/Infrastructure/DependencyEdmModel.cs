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

            #region Picture model

            builder.EntitySet<PictureDto>("Picture");
            builder.EntityType<PictureDto>().Count().Filter().OrderBy().Page();

            #endregion
        }
        protected void RegisterProduct(ODataConventionModelBuilder builder)
        {
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

            //insert/update/delete category
            #region Product category
            ActionConfiguration createCategory = product.Action("CreateProductCategory");
            createCategory.Parameter<ProductCategoryDto>("productCategory");
            createCategory.Returns<bool>();

            ActionConfiguration updateCategory = product.Action("UpdateProductCategory");
            updateCategory.Parameter<ProductCategoryDto>("productCategory");
            updateCategory.Returns<bool>();

            ActionConfiguration deleteCategory = product.Action("DeleteProductCategory");
            deleteCategory.Parameter<string>("CategoryId").Required();
            deleteCategory.Returns<bool>();
            #endregion

            //insert/update/delete manufacturer
            #region Product manufacturer
            ActionConfiguration createManufacturer = product.Action("CreateProductManufacturer");
            createManufacturer.Parameter<ProductManufacturerDto>("productManufacturer");
            createManufacturer.Returns<bool>();

            ActionConfiguration updateManufacturer = product.Action("UpdateProductManufacturer");
            updateManufacturer.Parameter<ProductManufacturerDto>("productManufacturer");
            updateManufacturer.Returns<bool>();

            ActionConfiguration deleteManufacturer = product.Action("DeleteProductManufacturer");
            deleteManufacturer.Parameter<string>("ManufacturerId").Required();
            deleteManufacturer.Returns<bool>();
            #endregion

            //insert/update/delete picture
            #region Product picture
            ActionConfiguration createPicture = product.Action("CreateProductPicture");
            createPicture.Parameter<ProductPictureDto>("productPicture");
            createPicture.Returns<bool>();

            ActionConfiguration updatePicture = product.Action("UpdateProductPicture");
            updatePicture.Parameter<ProductPictureDto>("productPicture");
            updatePicture.Returns<bool>();

            ActionConfiguration deletePicture = product.Action("DeleteProductPicture");
            deletePicture.Parameter<string>("PictureId").Required();
            deletePicture.Returns<bool>();
            #endregion

            #region Product specification
            ActionConfiguration createSpecification = product.Action("CreateProductSpecification");
            createSpecification.Parameter<ProductSpecificationAttributeDto>("productSpecification");
            createSpecification.Returns<bool>();

            ActionConfiguration updateSpecification = product.Action("UpdateProductSpecification");
            updateSpecification.Parameter<ProductSpecificationAttributeDto>("productSpecification");
            updateSpecification.Returns<bool>();

            ActionConfiguration deleteSpecification = product.Action("DeleteProductSpecification");
            deleteSpecification.Parameter<string>("Id").Required();
            deleteSpecification.Returns<bool>();
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

            ActionConfiguration changePassword = customer.Action("SetPassword");
            changePassword.Parameter<string>("password");
            changePassword.Returns<bool>();

            #endregion

            #region Customer Role model

            builder.EntitySet<CustomerRoleDto>("CustomerRole");
            builder.EntityType<CustomerRoleDto>().Count().Filter().OrderBy().Page();

            #endregion

            #region Vendors

            builder.EntitySet<VendorDto>("Vendor");
            builder.EntityType<VendorDto>().Count().Filter().OrderBy().Page();

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
                RegisterProduct(builder);
                RegisterCatalog(builder);
                RegisterCustomers(builder);
                RegisterShipping(builder);
            }
        }

        public int Order => 0;
    }
}
