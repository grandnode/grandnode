using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Catalog;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class TierPriceProfile : Profile, IMapperProfile
    {
        public TierPriceProfile()
        {
            CreateMap<TierPrice, ProductModel.TierPriceModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore());

            CreateMap<ProductModel.TierPriceModel, TierPrice>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}