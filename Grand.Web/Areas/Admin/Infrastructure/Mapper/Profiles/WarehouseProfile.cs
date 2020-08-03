using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class WarehouseProfile : Profile, IMapperProfile
    {
        public WarehouseProfile()
        {
            CreateMap<Warehouse, WarehouseModel>()
                .ForMember(dest => dest.Address, mo => mo.Ignore());

            CreateMap<WarehouseModel, Warehouse>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}