using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Mapper;
using Grand.Web.Areas.Admin.Models.Templates;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CategoryTemplateProfile : Profile, IAutoMapperProfile
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