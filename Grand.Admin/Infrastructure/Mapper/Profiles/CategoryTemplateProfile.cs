using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Templates;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class CategoryTemplateProfile : Profile, IMapperProfile
    {
        public CategoryTemplateProfile()
        {
            CreateMap<CategoryTemplate, CategoryTemplateModel>();
            CreateMap<CategoryTemplateModel, CategoryTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}