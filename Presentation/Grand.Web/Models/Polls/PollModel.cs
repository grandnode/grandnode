﻿using System;
using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Polls
{
    public partial class PollModel : BaseNopEntityModel, ICloneable
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

    public partial class PollAnswerModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string PollId { get; set; }
        public int NumberOfVotes { get; set; }

        public double PercentOfTotalVotes { get; set; }
    }
}