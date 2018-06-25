using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Bindings;
using Grand.Core.Data;

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

        public MongoDBContext(IMongoClient client)
        {
            string connectionString = DataSettingsHelper.ConnectionString();
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            _database = client.GetDatabase(databaseName);
            _client = client;
        }

        public MongoDBContext(IMongoClient client, IMongoDatabase mongodatabase)
        {
            _database = mongodatabase;
            _client = client;
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
            var writeBinding = new WritableServerBinding(_client.Cluster, NoCoreSession.NewHandle());
            return operation.Execute(writeBinding, CancellationToken.None);
        }

        public Task<BsonValue> RunScriptAsync(string command, CancellationToken cancellationToken)
        {
            var script = new BsonJavaScript(command);
            var operation = new EvalOperation(_database.DatabaseNamespace, script, null);
            var writeBinding = new WritableServerBinding(_client.Cluster, NoCoreSession.NewHandle());
            return operation.ExecuteAsync(writeBinding, CancellationToken.None);

        }
    }
}
