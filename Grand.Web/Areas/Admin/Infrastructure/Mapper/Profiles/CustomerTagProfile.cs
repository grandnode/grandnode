using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerTagProfile : Profile, IMapperProfile
    {
        public CustomerTagProfile()
        {
            CreateMap<CustomerTag, CustomerTagModel>();
            CreateMap<CustomerTagModel, CustomerTag>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}