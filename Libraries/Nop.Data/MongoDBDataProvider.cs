﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Hosting;
using Nop.Core.Data;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Nop.Data
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
             new DateTimeSerializer(DateTimeKind.Unspecified));
        }
        
        #endregion
    }
}
