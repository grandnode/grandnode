using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Domain.Tasks;
using Grand.Web.Areas.Admin.Models.Tasks;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ScheduleTaskProfile : Profile, IMapperProfile
    {
        public ScheduleTaskProfile()
        {
            CreateMap<ScheduleTask, ScheduleTaskModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore());

            CreateMap<ScheduleTaskModel, ScheduleTask>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Type, mo => mo.Ignore())
                .ForMember(dest => dest.ScheduleTaskName, mo => mo.Ignore())
                .ForMember(dest => dest.LastNonSuccessEndUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LastStartUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LeasedUntilUtc, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.LastSuccessUtc, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}
