using Grand.Domain.Catalog;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;
using Grand.Domain.Common;

namespace Grand.Web.Models.Common
{
    public partial class ContactUsModel : BaseModel
    {
        public ContactUsModel()
        {
            ContactAttributes = new List<ContactAttributeModel>();
            ContactAttribute = new List<CustomAttribute>();
        }

        [GrandResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("ContactUs.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        [GrandResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [GrandResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

        public string ContactAttributeInfo { get; set; }
        public IList<CustomAttribute> ContactAttribute { get; set; }
        public IList<ContactAttributeModel> ContactAttributes { get; set; }

        public partial class ContactAttributeModel : BaseEntityModel
        {
            public ContactAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ContactAttributeValueModel>();
            }

            public string Name { get; set; }

            public string DefaultValue { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ContactAttributeValueModel> Values { get; set; }
        }

        public partial class ContactAttributeValueModel : BaseEntityModel
        {
            public string Name { get; set; }

            public int DisplayOrder { get; set; }

            public string ColorSquaresRgb { get; set; }


            public bool IsPreSelected { get; set; }
        }
    }
}