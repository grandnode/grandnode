using AutoMapper;
using Grand.Domain.Courses;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Courses;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
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