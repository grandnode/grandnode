using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ProductProfile : Profile, IMapperProfile
    {

        public ProductProfile()
        {
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.ProductTypeId, mo => mo.MapFrom(src => (int)src.ProductType))
                .ForMember(dest => dest.BackorderModeId, mo => mo.MapFrom(src => (int)src.BackorderMode))
                .ForMember(dest => dest.DownloadActivationTypeId, mo => mo.MapFrom(src => (int)src.DownloadActivationType))
                .ForMember(dest => dest.IntervalUnitType, mo => mo.MapFrom(src => (int)src.IntervalUnitType))
                .ForMember(dest => dest.GiftCardTypeId, mo => mo.MapFrom(src => (int)src.GiftCardType))
                .ForMember(dest => dest.LowStockActivityId, mo => mo.MapFrom(src => (int)src.LowStockActivity))
                .ForMember(dest => dest.ManageInventoryMethodId, mo => mo.MapFrom(src => (int)src.ManageInventoryMethod))
                .ForMember(dest => dest.RecurringCyclePeriodId, mo => mo.MapFrom(src => (int)src.RecurringCyclePeriod))
                .ForMember(dest => dest.StockQuantity, mo => mo.Ignore())
                .ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
                .ForMember(dest => dest.ProductManufacturers, mo => mo.Ignore())
                .ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.ProductAttributeMappings, mo => mo.Ignore())
                .ForMember(dest => dest.ProductAttributeCombinations, mo => mo.Ignore())
                .ForMember(dest => dest.TierPrices, mo => mo.Ignore())
                .ForMember(dest => dest.ProductWarehouseInventory, mo => mo.Ignore())
                .ForMember(dest => dest.CrossSellProduct, mo => mo.Ignore())
                .ForMember(dest => dest.RelatedProducts, mo => mo.Ignore())
                .ForMember(dest => dest.BundleProducts, mo => mo.Ignore())
                .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                .ForMember(dest => dest.SubjectToAcl, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Product, ProductDto>();

        }

        public int Order => 1;
    }
}
