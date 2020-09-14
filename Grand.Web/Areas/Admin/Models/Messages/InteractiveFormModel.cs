﻿using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class InteractiveFormModel : BaseEntityModel, ILocalizedModel<InteractiveFormLocalizedModel>
    {
        public InteractiveFormModel()
        {
            Locales = new List<InteractiveFormLocalizedModel>();
            AvailableEmailAccounts = new List<EmailAccountModel>();
        }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.Body")]

        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.EmailAccount")]
        public string EmailAccountId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.AvailableTokens")]
        public string AvailableTokens { get; set; }
        public IList<EmailAccountModel> AvailableEmailAccounts { get; set; }

        public IList<InteractiveFormLocalizedModel> Locales { get; set; }

    }

    public partial class InteractiveFormLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Fields.Body")]

        public string Body { get; set; }

    }

}