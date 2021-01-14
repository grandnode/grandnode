using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Domain.Customers;
using Grand.Admin.Models.Customers;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class SalesEmployeeProfile : Profile, IMapperProfile
    {
        public SalesEmployeeProfile()
        {
            CreateMap<SalesEmployee, SalesEmployeeModel>();

            CreateMap<SalesEmployeeModel, SalesEmployee>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}