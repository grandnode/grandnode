using System;
using Grand.Core.Data;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Grand.Data
{
    public class MongoDBDataProvider : IDataProvider
    {
        #region Methods


        /// <summary>
        /// Initialize database
        /// </summary>
        public virtual void InitDatabase()
        {
            DataSettingsHelper.InitConnectionString();
        }

        #endregion
    }
}
