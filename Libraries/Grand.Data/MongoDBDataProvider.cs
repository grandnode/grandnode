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

        /// <summary>
        /// Set database initializer
        /// </summary>
        public virtual void SetDatabaseInitializer()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTime),
             new DateTimeSerializer(DateTimeKind.Utc));
        }

        #endregion
    }
}
