using MongoDB.Bson.Serialization.Conventions;

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
    }
}
