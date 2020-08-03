using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class PickupPointProfile : Profile, IMapperProfile
    {
        public PickupPointProfile()
        {
            CreateMap<PickupPoint, PickupPointModel>()
                .ForMember(dest => dest.Address, mo => mo.Ignore());

            CreateMap<PickupPointModel, PickupPoint>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}