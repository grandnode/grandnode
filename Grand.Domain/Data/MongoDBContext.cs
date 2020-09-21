using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Domain.Data
{
    public class MongoDBContext : IMongoDBContext
    {
        protected IMongoDatabase _database;
        public MongoDBContext()
        {

        }

        public MongoDBContext(IMongoDatabase mongodatabase)
        {
            _database = mongodatabase;
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
            var writeBinding = new WritableServerBinding(_database.Client.Cluster, NoCoreSession.NewHandle());
            return operation.Execute(writeBinding, CancellationToken.None);
        }

        public Task<BsonValue> RunScriptAsync(string command, CancellationToken cancellationToken)
        {
            var script = new BsonJavaScript(command);
            var operation = new EvalOperation(_database.DatabaseNamespace, script, null);
            var writeBinding = new WritableServerBinding(_database.Client.Cluster, NoCoreSession.NewHandle());
            return operation.ExecuteAsync(writeBinding, CancellationToken.None);
        }
    }
}
