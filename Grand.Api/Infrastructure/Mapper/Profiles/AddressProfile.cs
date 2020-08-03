using AutoMapper;
using Grand.Api.DTOs.Customers;
using Grand.Domain.Common;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class AddressProfile : Profile, IMapperProfile
    {
        public AddressProfile()
        {
            CreateMap<AddressDto, Address>()
                .ForMember(dest => dest.CustomAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Address, AddressDto>();
        }

        public int Order => 1;
    }
}
