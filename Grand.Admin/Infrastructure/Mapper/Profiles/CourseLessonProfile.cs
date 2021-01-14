using AutoMapper;
using Grand.Domain.Courses;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Courses;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class CourseLessonProfile : Profile, IMapperProfile
    {
        public CourseLessonProfile()
        {
            CreateMap<CourseLesson, CourseLessonModel>()
                .ForMember(dest => dest.AvailableSubjects, mo => mo.Ignore());
            CreateMap<CourseLessonModel, CourseLesson>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}