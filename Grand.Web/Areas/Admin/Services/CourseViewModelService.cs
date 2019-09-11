using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Seo;
using Grand.Services.Courses;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Courses;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CourseViewModelService : ICourseViewModelService
    {
        private readonly ICourseService _courseService;
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly SeoSettings _seoSettings;

        public CourseViewModelService(ICourseService courseService, ICourseLevelService courseLevelService, ICourseLessonService courseLessonService,
            ICourseSubjectService courseSubjectService,
            IUrlRecordService urlRecordService, IPictureService pictureService, ILanguageService languageService,
            ILocalizationService localizationService, ICustomerActivityService customerActivityService,
            SeoSettings seoSettings)
        {
            _courseService = courseService;
            _courseLevelService = courseLevelService;
            _courseLessonService = courseLessonService;
            _courseSubjectService = courseSubjectService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _languageService = languageService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _seoSettings = seoSettings;
        }

        public virtual async Task<CourseModel> PrepareCourseModel(CourseModel model = null)
        {
            if (model == null)
            {
                model = new CourseModel();
                model.Published = true;
            }

            foreach (var item in await _courseLevelService.GetAll())
            {
                model.AvailableLevels.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() {
                    Text = item.Name,
                    Value = item.Id
                });
            }

            return model;
        }

        public virtual async Task<Course> InsertCourseModel(CourseModel model)
        {
            var course = model.ToEntity();
            course.CreatedOnUtc = DateTime.UtcNow;
            course.UpdatedOnUtc = DateTime.UtcNow;

            await _courseService.Insert(course);

            //locales
            model.SeName = await course.ValidateSeName(model.SeName, course.Name, true, _seoSettings, _urlRecordService, _languageService);
            course.SeName = model.SeName;
            await _courseService.Update(course);

            await _urlRecordService.SaveSlug(course, model.SeName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(course.PictureId, course.Name);

            //activity log
            await _customerActivityService.InsertActivity("AddNewCourse", course.Id, _localizationService.GetResource("ActivityLog.AddNewCourse"), course.Name);

            return course;
        }

        public virtual async Task<Course> UpdateCourseModel(Course course, CourseModel model)
        {
            string prevPictureId = course.PictureId;
            course = model.ToEntity(course);
            course.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = await course.ValidateSeName(model.SeName, course.Name, true, _seoSettings, _urlRecordService, _languageService);
            course.SeName = model.SeName;
            //locales
            course.Locales = await model.Locales.ToLocalizedProperty(course, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            await _courseService.Update(course);
            //search engine name
            await _urlRecordService.SaveSlug(course, model.SeName, "");

            //delete an old picture (if deleted or updated)
            if (!string.IsNullOrEmpty(prevPictureId) && prevPictureId != course.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(course.PictureId, course.Name);

            //activity log
            await _customerActivityService.InsertActivity("EditCourse", course.Id, _localizationService.GetResource("ActivityLog.EditCourse"), course.Name);

            return course;
        }
        public virtual async Task DeleteCourse(Course course)
        {
            await _courseService.Delete(course);
            //activity log
            await _customerActivityService.InsertActivity("DeleteCourse", course.Id, _localizationService.GetResource("ActivityLog.DeleteCourse"), course.Name);
        }

        public virtual async Task<CourseLessonModel> PrepareCourseLessonModel(string courseId, CourseLessonModel model = null)
        {
            if (model == null)
            {
                model = new CourseLessonModel();
                model.Published = true;
            }
            model.CourseId = courseId;

            foreach (var item in await _courseSubjectService.GetByCourseId(courseId))
            {
                model.AvailableSubjects.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() {
                    Text = item.Name,
                    Value = item.Id
                });
            }

            return model;
        }

        public virtual async Task<CourseLesson> InsertCourseLessonModel(CourseLessonModel model)
        {
            var lesson = model.ToEntity();
            await _courseLessonService.Insert(lesson);
            //activity log
            await _customerActivityService.InsertActivity("AddNewCourseLesson", lesson.Id, _localizationService.GetResource("ActivityLog.AddNewCourseLesson"), lesson.Name);
            return lesson;
        }

        public virtual async Task<CourseLesson> UpdateCourseLessonModel(CourseLesson lesson, CourseLessonModel model)
        {
            string prevPictureId = lesson.PictureId;
            lesson = model.ToEntity(lesson);
            await _courseLessonService.Update(lesson);

            //delete an old picture (if deleted or updated)
            if (!string.IsNullOrEmpty(prevPictureId) && prevPictureId != lesson.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }

            //activity log
            await _customerActivityService.InsertActivity("EditCourseLesson", lesson.Id, _localizationService.GetResource("ActivityLog.EditLessonCourse"), lesson.Name);

            return lesson;
        }
        public virtual async Task DeleteCourseLesson(CourseLesson lesson)
        {
            await _courseLessonService.Delete(lesson);
            //activity log
            await _customerActivityService.InsertActivity("DeleteCourseLesson", lesson.Id, _localizationService.GetResource("ActivityLog.DeleteCourseLesson"), lesson.Name);
        }

    }
}
