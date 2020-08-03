using AutoMapper;
using Grand.Domain.Common;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class AddressAttributeProfile : Profile, IMapperProfile
    {
        public AddressAttributeProfile()
        {
            CreateMap<AddressAttribute, AddressAttributeModel>()
                .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<AddressAttributeModel, AddressAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.AddressAttributeValues, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}