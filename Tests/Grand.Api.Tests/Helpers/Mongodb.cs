using Grand.Services.Tests;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Grand.Api.Tests.Helpers
{
    public static class Mongodb
    {
        public static void IgnoreExtraElements()
        {
            var cp = new ConventionPack();
            cp.Add(new IgnoreExtraElementsConvention(true));
            ConventionRegistry.Register("ApplicationConventions", cp, t => true);
        }

        public static (IMongoClient client, IMongoDatabase database) MongoDbClient()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            return (client, database);
        }
    }
}
