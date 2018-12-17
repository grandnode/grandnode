using AutoMapper;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ApiMapperModelConfiguration : Profile, IMapperProfile
    {

        public ApiMapperModelConfiguration()
        {
            CreateMap<Grand.Api.Model.Catalog.Category, Grand.Core.Domain.Catalog.Category>()
                .ForMember(dest => dest.SubjectToAcl, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Grand.Core.Domain.Catalog.Category, Grand.Api.Model.Catalog.Category>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
        }

        public int Order => 1;
    }
}
