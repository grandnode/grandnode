using System;
using Grand.Core;
using Grand.Core.Data;

namespace Grand.Data
{
    public partial class MongoDBDataProviderManager : BaseDataProviderManager
    {
        public MongoDBDataProviderManager(DataSettings settings):base(settings)
        {
        }

        public override IDataProvider LoadDataProvider()
        {

            var providerName = Settings.DataProvider;
            if (String.IsNullOrWhiteSpace(providerName))
                throw new GrandException("Data Settings doesn't contain a providerName");

            switch (providerName.ToLowerInvariant())
            {
                case "mongodb":
                    return new MongoDBDataProvider();
                default:
                    throw new GrandException(string.Format("Not supported dataprovider name: {0}", providerName));
            }
        }

    }
}
