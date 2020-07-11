using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerAttributeProfile : Profile, IMapperProfile
    {
        public CustomerAttributeProfile()
        {
            CreateMap<CustomerAttribute, CustomerAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<CustomerAttributeModel, CustomerAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerAttributeValues, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}