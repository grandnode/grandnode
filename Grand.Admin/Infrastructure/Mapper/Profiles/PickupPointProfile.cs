using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Shipping;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class PickupPointProfile : Profile, IMapperProfile
    {
        public PickupPointProfile()
        {
            CreateMap<PickupPoint, PickupPointModel>()
                .ForMember(dest => dest.Address, mo => mo.MapFrom(y => y.Address));

            CreateMap<PickupPointModel, PickupPoint>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}