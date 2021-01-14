using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Polls
{
    public partial class PollAnswerModel : BaseEntityModel, ILocalizedModel<PollAnswerLocalizedModel>
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