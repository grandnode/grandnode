using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Bindings;

namespace Grand.Data
{
    public class MongoDBContext : IMongoDBContext
    {
        protected IMongoDatabase _database;
        protected IMongoClient _client;

        public MongoDBContext()
        {

        }
        public MongoDBContext(string connectionString)
        {
            _client = new MongoClient(connectionString);
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            _database = _client.GetDatabase(databaseName);
        }

        public IMongoClient Client()
        {
            return _client;
        }

        public IMongoDatabase Database()
        {
            return _database;
        }

        public TResult RunCommand<TResult>(string command)
        {
            return _database.RunCommand<TResult>(command);
        }

        public TResult RunCommand<TResult>(string command, ReadPreference readpreference)
        {
            return _database.RunCommand<TResult>(command, readpreference);
        }

        public BsonValue RunScript(string command, CancellationToken cancellationToken)
        {
            var script = new BsonJavaScript(command);
            var operation = new EvalOperation(_database.DatabaseNamespace, script, null);
            var writeBinding = new WritableServerBinding(_client.Cluster);
            return operation.Execute(writeBinding, CancellationToken.None);

        }

        public Task<BsonValue> RunScriptAsync(string command, CancellationToken cancellationToken)
        {
            var script = new BsonJavaScript(command);
            var operation = new EvalOperation(_database.DatabaseNamespace, script, null);
            var writeBinding = new WritableServerBinding(_client.Cluster);
            return operation.ExecuteAsync(writeBinding, CancellationToken.None);

        }
    }
}
