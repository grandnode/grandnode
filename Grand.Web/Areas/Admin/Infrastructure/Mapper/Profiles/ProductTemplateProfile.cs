using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Templates;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ProductTemplateProfile : Profile, IMapperProfile
    {
        public ProductTemplateProfile()
        {
            CreateMap<ProductTemplate, ProductTemplateModel>();
            CreateMap<ProductTemplateModel, ProductTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}