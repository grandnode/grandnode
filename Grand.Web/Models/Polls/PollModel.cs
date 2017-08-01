using System;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Polls
{
    public partial class PollModel : BaseGrandEntityModel, ICloneable
    {
        public PollModel()
        {
            Answers = new List<PollAnswerModel>();
        }

        public string Name { get; set; }

        public bool AlreadyVoted { get; set; }

        public int TotalVotes { get; set; }
        
        public IList<PollAnswerModel> Answers { get; set; }

        public object Clone()
        {
            //we use a shallow copy (deep clone is not required here)
            return this.MemberwiseClone();
        }
    }

    public partial class PollAnswerModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string PollId { get; set; }
        public int NumberOfVotes { get; set; }

        public double PercentOfTotalVotes { get; set; }
    }
}