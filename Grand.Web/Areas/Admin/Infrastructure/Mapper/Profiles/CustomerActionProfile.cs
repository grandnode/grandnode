using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerActionProfile : Profile, IMapperProfile
    {
        public CustomerActionProfile()
        {
            CreateMap<CustomerAction, CustomerActionModel>()
                .ForMember(dest => dest.MessageTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Banners, mo => mo.Ignore());

            CreateMap<CustomerActionModel, CustomerAction>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            CreateMap<CustomerAction.ActionCondition, CustomerActionConditionModel>()
                .ForMember(dest => dest.CustomerActionConditionType, mo => mo.Ignore());
            CreateMap<CustomerActionConditionModel, CustomerAction.ActionCondition>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerActionConditionType, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}