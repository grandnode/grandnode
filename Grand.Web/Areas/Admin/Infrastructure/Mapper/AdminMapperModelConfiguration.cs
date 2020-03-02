using AutoMapper;
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
using Grand.Services.Payments;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Extensions;
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
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper
{
    public class AdminMapperModelConfiguration : Profile, IMapperProfile
    {
        public AdminMapperModelConfiguration()
        {
            //product template
            CreateMap<ProductTemplate, ProductTemplateModel>();
            CreateMap<ProductTemplateModel, ProductTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
            //topic template
            CreateMap<TopicTemplate, TopicTemplateModel>();
            CreateMap<TopicTemplateModel, TopicTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            //document type
            CreateMap<DocumentType, DocumentTypeModel>();
            CreateMap<DocumentTypeModel, DocumentType>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            //documents
            CreateMap<Document, DocumentModel>()
               .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
               .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
               .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableDocumentTypes, mo => mo.Ignore());
            CreateMap<DocumentModel, Document>()
               .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
               .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
               .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(x => x.SelectedCustomerRoleIds != null ? x.SelectedCustomerRoleIds.ToList() : new List<string>()))
               .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
               .ForMember(dest => dest.Id, mo => mo.Ignore());

            //course level
            CreateMap<CourseLevel, CourseLevelModel>();
            CreateMap<CourseLevelModel, CourseLevel>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            //course subject
            CreateMap<CourseSubject, CourseSubjectModel>();
            CreateMap<CourseSubjectModel, CourseSubject>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            //course
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableLevels, mo => mo.Ignore())
                .ForMember(dest => dest.ProductName, mo => mo.Ignore());

            CreateMap<CourseModel, Course>()
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(x => x.SelectedCustomerRoleIds != null ? x.SelectedCustomerRoleIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            //course lesson
            CreateMap<CourseLesson, CourseLessonModel>()
                .ForMember(dest => dest.AvailableSubjects, mo => mo.Ignore());
            CreateMap<CourseLessonModel, CourseLesson>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

        }

        public int Order {
            get { return 0; }
        }
    }
}