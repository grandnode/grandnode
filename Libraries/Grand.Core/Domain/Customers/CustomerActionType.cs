﻿using System;
using Grand.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Grand.Core.Domain.Customers
{

    [BsonIgnoreExtraElements]
    public partial class CustomerActionType : BaseEntity
    {
        private ICollection<int> _conditionType;

        public string SystemKeyword { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public virtual ICollection<int> ConditionType
        {
            get { return _conditionType ?? (_conditionType = new List<int>()); }
            protected set { _conditionType = value; }
        }


    }


}
