using AutoMapper;
using Grand.Domain.Directory;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class MeasureUnitProfile : Profile, IMapperProfile
    {
        public MeasureUnitProfile()
        {
            CreateMap<MeasureUnit, MeasureUnitModel>();
            CreateMap<MeasureUnitModel, MeasureUnit>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}