using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Documents;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Services.Cms;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Cms;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Courses;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Web.Areas.Admin.Models.Documents;
using Grand.Web.Areas.Admin.Models.ExternalAuthentication;
using Grand.Web.Areas.Admin.Models.Forums;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Grand.Web.Areas.Admin.Models.Localization;
using Grand.Web.Areas.Admin.Models.Logging;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Models.News;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Models.Payments;
using Grand.Web.Areas.Admin.Models.Plugins;
using Grand.Web.Areas.Admin.Models.Polls;
using Grand.Web.Areas.Admin.Models.Settings;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Models.Tax;
using Grand.Web.Areas.Admin.Models.Templates;
using Grand.Web.Areas.Admin.Models.Topics;
using Grand.Web.Areas.Admin.Models.Vendors;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        #region Course

        public static CourseModel ToModel(this Course entity)
        {
            return entity.MapTo<Course, CourseModel>();
        }

        public static Course ToEntity(this CourseModel model)
        {
            return model.MapTo<CourseModel, Course>();
        }

        public static Course ToEntity(this CourseModel model, Course destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Course subject

        public static CourseSubjectModel ToModel(this CourseSubject entity)
        {
            return entity.MapTo<CourseSubject, CourseSubjectModel>();
        }

        public static CourseSubject ToEntity(this CourseSubjectModel model)
        {
            return model.MapTo<CourseSubjectModel, CourseSubject>();
        }

        public static CourseSubject ToEntity(this CourseSubjectModel model, CourseSubject destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Course lesson

        public static CourseLessonModel ToModel(this CourseLesson entity)
        {
            return entity.MapTo<CourseLesson, CourseLessonModel>();
        }

        public static CourseLesson ToEntity(this CourseLessonModel model)
        {
            return model.MapTo<CourseLessonModel, CourseLesson>();
        }

        public static CourseLesson ToEntity(this CourseLessonModel model, CourseLesson destination)
        {
            return model.MapTo(destination);
        }

        #endregion
    }
}