using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Hosting;
using Nop.Core.Data;

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

        #endregion
    }
}
