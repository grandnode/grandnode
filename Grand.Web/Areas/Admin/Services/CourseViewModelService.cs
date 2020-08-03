using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Courses;
using Grand.Domain.Seo;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Courses;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IProductCourseService _productCourseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly SeoSettings _seoSettings;

        public CourseViewModelService(ICourseService courseService, ICourseLevelService courseLevelService, ICourseLessonService courseLessonService,
            ICourseSubjectService courseSubjectService,
            IUrlRecordService urlRecordService, IPictureService pictureService, ILanguageService languageService,
            ILocalizationService localizationService, ICustomerActivityService customerActivityService, IProductCourseService productCourseService,
            IServiceProvider serviceProvider,
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
            _productCourseService = productCourseService;
            _serviceProvider = serviceProvider;
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
            if (!string.IsNullOrEmpty(model.ProductId))
            {
                var productService = _serviceProvider.GetRequiredService<IProductService>();
                model.ProductName = (await productService.GetProductById(model.ProductId))?.Name;
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

            //course on the product 
            if (!string.IsNullOrEmpty(course.ProductId))
                await _productCourseService.UpdateCourseOnProduct(course.ProductId, course.Id);


            //activity log
            await _customerActivityService.InsertActivity("AddNewCourse", course.Id, _localizationService.GetResource("ActivityLog.AddNewCourse"), course.Name);

            return course;
        }

        public virtual async Task<Course> UpdateCourseModel(Course course, CourseModel model)
        {
            string prevPictureId = course.PictureId;
            string prevProductId = course.ProductId;

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

            //course on the product 
            if (!string.IsNullOrEmpty(prevProductId))
                await _productCourseService.UpdateCourseOnProduct(prevProductId, string.Empty);

            if (!string.IsNullOrEmpty(course.ProductId))
                await _productCourseService.UpdateCourseOnProduct(course.ProductId, course.Id);


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

        public virtual async Task<CourseModel.AssociateProductToCourseModel> PrepareAssociateProductToCourseModel()
        {
            var model = new CourseModel.AssociateProductToCourseModel();
            //a vendor should have access only to his products
            var workContext = _serviceProvider.GetRequiredService<IWorkContext>();
            model.IsLoggedInAsVendor = workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categoryService = _serviceProvider.GetRequiredService<ICategoryService>();
            var categories = await categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var manufacturerService = _serviceProvider.GetRequiredService<IManufacturerService>();
            foreach (var m in await manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            foreach (var s in await storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var vendorService = _serviceProvider.GetRequiredService<IVendorService>();
            foreach (var v in await vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList().ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            return model;
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CourseModel.AssociateProductToCourseModel model, int pageIndex, int pageSize)
        {
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var dateTimeHelper = _serviceProvider.GetRequiredService<IDateTimeHelper>();
            var products = await productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(dateTimeHelper)).ToList(), products.TotalCount);
        }
    }
}
