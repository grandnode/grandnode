using AutoMapper;
using Grand.Domain.Logging;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Logging;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ActivityLogTypeProfile : Profile, IMapperProfile
    {
        public ActivityLogTypeProfile()
        {
            CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());

            CreateMap<ActivityLogType, ActivityLogTypeModel>();

            CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore());

            CreateMap<ActivityStats, ActivityStatsModel>()
                .ForMember(dest => dest.ActivityLogTypeName, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}