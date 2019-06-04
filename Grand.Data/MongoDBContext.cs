using Grand.Core.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Data
{
    public class MongoDBContext : IMongoDBContext
    {
        protected IMongoDatabase _database;
        protected IMongoClient _client;

        /// <summary>
        /// Get a default instance by data settings
        /// </summary>
        public MongoDBContext()
        {
            _client = DataSettingsHelper.MongoClient();
            _database = _client.GetDatabase();
        }
        public MongoDBContext(IMongoClient client)
        {
            _database = client.GetDatabase();
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
