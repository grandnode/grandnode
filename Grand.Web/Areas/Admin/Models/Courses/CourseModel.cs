using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Courses
{
    [Validator(typeof(CourseValidator))]
    public partial class CourseModel : BaseGrandEntityModel, ILocalizedModel<CourseLocalizedModel>, IAclMappingModel, IStoreMappingModel
    {
        public CourseModel()
        {
            Locales = new List<CourseLocalizedModel>();
            AvailableLevels = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.Published")]
        public bool Published { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.Courses.Course.Fields.PictureId")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.LevelId")]
        public string LevelId { get; set; }
        public IList<SelectListItem> AvailableLevels { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.Courses.Course.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Courses.Course.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Courses.Course.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Courses.Course.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.SeName")]

        public string SeName { get; set; }

        public IList<CourseLocalizedModel> Locales { get; set; }        
    }

    public partial class CourseLocalizedModel : ILocalizedModelLocal, ISlugModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Fields.SeName")]

        public string SeName { get; set; }

    }
}
