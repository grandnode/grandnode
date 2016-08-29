using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Grand.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Grand.Core
{
    public abstract class ParentEntity
    {
        public ParentEntity()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }

        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _id = ObjectId.GenerateNewId().ToString();
                else
                    _id = value;
            }
        }

        private string _id;

    }
}
