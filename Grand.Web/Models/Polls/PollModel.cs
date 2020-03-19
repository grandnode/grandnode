using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Polls
{
    public partial class PollModel : BaseGrandEntityModel
    {
        public PollModel()
        {
            Answers = new List<PollAnswerModel>();
        }

        public string Name { get; set; }

        public bool AlreadyVoted { get; set; }

        public int TotalVotes { get; set; }
        
        public IList<PollAnswerModel> Answers { get; set; }

    }

    public partial class PollAnswerModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string PollId { get; set; }
        public int NumberOfVotes { get; set; }

        public double PercentOfTotalVotes { get; set; }
    }
}