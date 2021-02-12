using AutoMapper;
using Grand.Core.Mapper;
using Grand.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class SalesEmployeeProfile : Profile, IAutoMapperProfile
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