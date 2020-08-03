using Grand.Services.Courses;
using Grand.Services.Media;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Courses;
using Grand.Web.Models.Course;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Courses
{
    public class GetLessonHandler : IRequestHandler<GetLesson, LessonModel>
    {
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseActionService _courseActionService;
        private readonly IPictureService _pictureService;

        public GetLessonHandler(
            ICourseLevelService courseLevelService,
            ICourseActionService courseActionService,
            IPictureService pictureService)
        {
            _courseLevelService = courseLevelService;
            _courseActionService = courseActionService;
            _pictureService = pictureService;
        }

        public async Task<LessonModel> Handle(GetLesson request, CancellationToken cancellationToken)
        {
            var model = new LessonModel();

            var modelCourse = request.Course.ToModel(request.Language);
            model.Id = request.Lesson.Id;
            model.CourseId = modelCourse.Id;
            model.CourseDescription = modelCourse.Description;
            model.CourseName = modelCourse.Name;
            model.CourseSeName = modelCourse.SeName;
            model.MetaDescription = modelCourse.MetaDescription;
            model.MetaKeywords = model.MetaKeywords;
            model.MetaTitle = model.MetaTitle;
            model.Name = request.Lesson.Name;
            model.ShortDescription = request.Lesson.ShortDescription;
            model.Description = request.Lesson.Description;
            model.GenericAttributes = request.Lesson.GenericAttributes;
            model.CourseLevel = (await _courseLevelService.GetById(request.Course.LevelId))?.Name;

            //prepare picture
            model.PictureUrl = await _pictureService.GetPictureUrl(request.Lesson.PictureId);

            model.Approved = await _courseActionService.CustomerLessonCompleted(request.Customer.Id, request.Lesson.Id);
            if (!string.IsNullOrEmpty(request.Lesson.AttachmentId))
                model.DownloadFile = true;

            if (!string.IsNullOrEmpty(request.Lesson.VideoFile))
                model.VideoFile = true;

            return model;
        }
    }
}
