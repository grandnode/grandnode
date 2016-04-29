using MongoDB.Driver;
using Nop.Core;
using Nop.Core.Data;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Tests
{
    public partial class MongoDBRepositoryTest<T> : MongoDBRepository<T>, IRepository<T> where T : BaseEntity
    {
        public MongoDBRepositoryTest()
        {
            var client = DriverTestConfiguration.Client;
            _database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            _database.DropCollection(DriverTestConfiguration.CollectionNamespace.CollectionName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }
    }
}
