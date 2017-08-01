using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Polls;
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Localization;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Polls
{
    [Validator(typeof(PollAnswerValidator))]
    public partial class PollAnswerModel : BaseGrandEntityModel, ILocalizedModel<PollAnswerLocalizedModel>
    {
        public PollAnswerModel()
        {
            Locales = new List<PollAnswerLocalizedModel>();
        }

        public string PollId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.NumberOfVotes")]
        public int NumberOfVotes { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<PollAnswerLocalizedModel> Locales { get; set; }


    }

    public partial class PollAnswerLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.Name")]
        
        public string Name { get; set; }
    }


}