using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Templates;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ManufacturerTemplateProfile : Profile, IMapperProfile
    {
        public ManufacturerTemplateProfile()
        {
            CreateMap<ManufacturerTemplate, ManufacturerTemplateModel>();
            CreateMap<ManufacturerTemplateModel, ManufacturerTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}