using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerRoleProfile : Profile, IMapperProfile
    {
        public CustomerRoleProfile()
        {
            CreateMap<CustomerRole, CustomerRoleModel>();
            CreateMap<CustomerRoleModel, CustomerRole>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}