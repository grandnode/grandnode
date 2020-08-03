using AutoMapper;
using Grand.Domain.Directory;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class MeasureDimensionProfile : Profile, IMapperProfile
    {
        public MeasureDimensionProfile()
        {
            CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(dest => dest.IsPrimaryDimension, mo => mo.Ignore());

            CreateMap<MeasureDimensionModel, MeasureDimension>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}