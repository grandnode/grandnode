using AutoMapper;
using Grand.Domain.Directory;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class MeasureWeightProfile : Profile, IMapperProfile
    {
        public MeasureWeightProfile()
        {
            CreateMap<MeasureWeight, MeasureWeightModel>()
                .ForMember(dest => dest.IsPrimaryWeight, mo => mo.Ignore());

            CreateMap<MeasureWeightModel, MeasureWeight>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}