using MongoDB.Driver;

namespace Grand.Data
{
    public interface IMongoDBContext
    {
        IMongoDatabase Database();
        IMongoClient Client();
        TResult RunCommand<TResult>(string command);
        TResult RunCommand<TResult>(string command, ReadPreference readpreference);
    }
}
