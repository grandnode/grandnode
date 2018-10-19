using Grand.Core;
using Grand.Core.Data;
using System;

namespace Grand.Data
{
    public partial class MongoDBDataProviderManager : BaseDataProviderManager
    {
        public MongoDBDataProviderManager(DataSettings settings) : base(settings)
        {
        }

        public override IDataProvider LoadDataProvider()
        {
            var providerName = Settings.DataProvider;
            if (!String.IsNullOrWhiteSpace(providerName) && providerName.ToLowerInvariant() != "mongodb")
                throw new GrandException(string.Format("Not supported dataprovider name: {0}", providerName));

            return new MongoDBDataProvider();
        }
    }
}
