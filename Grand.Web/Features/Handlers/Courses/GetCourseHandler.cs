using Grand.Domain.Media;
using Grand.Services.Courses;
using Grand.Services.Media;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Courses;
using Grand.Web.Models.Course;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Grand.Web.Features.Handlers.Courses
{
    public class GetCourseHandler : IRequestHandler<GetCourse, CourseModel>
    {
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseActionService _courseActionService;
        private readonly IPictureService _pictureService;

        private readonly MediaSettings _mediaSettings;

        public GetCourseHandler(
            ICourseLevelService courseLevelService,
            ICourseSubjectService courseSubjectService,
            ICourseLessonService courseLessonService,
            ICourseActionService courseActionService,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _courseLevelService = courseLevelService;
            _courseSubjectService = courseSubjectService;
            _courseLessonService = courseLessonService;
            _courseActionService = courseActionService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public async Task<CourseModel> Handle(GetCourse request, CancellationToken cancellationToken)
        {
            var model = request.Course.ToModel(request.Language);
            model.Level = (await _courseLevelService.GetById(request.Course.LevelId))?.Name;

            model.PictureUrl = await _pictureService.GetPictureUrl(request.Course.PictureId, _mediaSettings.CourseThumbPictureSize);
            var subjects = await _courseSubjectService.GetByCourseId(request.Course.Id);
            foreach (var item in subjects)
            {
                model.Subjects.Add(new CourseModel.Subject() {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayOrder = item.DisplayOrder
                });
            }
            var lessons = await _courseLessonService.GetByCourseId(request.Course.Id);
            foreach (var item in lessons.Where(x => x.Published))
            {
                var pictureUrl = await _pictureService.GetPictureUrl(item.PictureId, _mediaSettings.LessonThumbPictureSize);
                var approved = await _courseActionService.CustomerLessonCompleted(request.Customer.Id, item.Id);

                model.Lessons.Add(new CourseModel.Lesson() {
                    Id = item.Id,
                    SubjectId = item.SubjectId,
                    Name = item.Name,
                    ShortDescription = item.ShortDescription,
                    DisplayOrder = item.DisplayOrder,
                    PictureUrl = pictureUrl,
                    Approved = approved
                });
            }
            model.Approved = !model.Lessons.Any(x => !x.Approved);
            return model;
        }
    }
}
