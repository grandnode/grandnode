using System;
using Nop.Core;
using Nop.Core.Data;

namespace Nop.Data
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
                throw new NopException("Data Settings doesn't contain a providerName");

            switch (providerName.ToLowerInvariant())
            {
                case "mongodb":
                    return new MongoDBDataProvider();
                default:
                    throw new NopException(string.Format("Not supported dataprovider name: {0}", providerName));
            }
        }

    }
}
