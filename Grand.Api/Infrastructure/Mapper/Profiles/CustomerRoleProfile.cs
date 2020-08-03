using AutoMapper;
using Grand.Api.DTOs.Customers;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class CustomerRoleProfile : Profile, IMapperProfile
    {
        public CustomerRoleProfile()
        {

            CreateMap<CustomerRoleDto, CustomerRole>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<CustomerRole, CustomerRoleDto>();

        }

        public int Order => 1;
    }
}
