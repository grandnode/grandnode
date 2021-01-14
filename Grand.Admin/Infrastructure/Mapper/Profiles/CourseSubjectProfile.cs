using AutoMapper;
using Grand.Domain.Courses;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Courses;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class CourseSubjectProfile : Profile, IMapperProfile
    {
        public CourseSubjectProfile()
        {
            CreateMap<CourseSubject, CourseSubjectModel>();
            CreateMap<CourseSubjectModel, CourseSubject>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}