using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Infrastructure;

namespace Grand.Data
{
    public class MongoDBStartUpTask : IStartupTask
    {
        public void Execute()
        {
            var settings = EngineContext.Current.Resolve<DataSettings>();
            if (settings != null && settings.IsValid())
            {
                var provider = EngineContext.Current.Resolve<IDataProvider>();
                if (provider == null)
                    throw new GrandException("No IDataProvider found");
            }
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return -1000; }
        }
    }
}
