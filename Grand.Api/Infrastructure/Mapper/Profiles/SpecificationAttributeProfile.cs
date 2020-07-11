using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class SpecificationAttributeProfile : Profile, IMapperProfile
    {
        public SpecificationAttributeProfile()
        {

            CreateMap<SpecificationAttributeDto, SpecificationAttribute>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<SpecificationAttribute, SpecificationAttributeDto>();

            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionDto>();

            CreateMap<SpecificationAttributeOptionDto, SpecificationAttributeOption>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

        }

        public int Order => 1;
    }
}
