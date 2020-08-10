using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Domain.Data
{
    public interface IMongoDBContext
    {
        IMongoDatabase Database();
        TResult RunCommand<TResult>(string command);
        TResult RunCommand<TResult>(string command, ReadPreference readpreference);
        BsonValue RunScript(string command, CancellationToken cancellationToken);
        Task<BsonValue> RunScriptAsync(string command, CancellationToken cancellationToken);
    }
}
