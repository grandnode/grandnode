using Grand.Core.Data;
using Grand.Domain;
using Grand.Domain.Data;

namespace Grand.Services.Tests
{
    public partial class MongoDBRepositoryTest<T> : Repository<T>, IRepository<T> where T : BaseEntity
    {
        public MongoDBRepositoryTest(): base(new MongoDBDataProvider())
        {
            var client = DriverTestConfiguration.Client;
            _database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            _database.DropCollection(DriverTestConfiguration.CollectionNamespace.CollectionName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }
    }
}