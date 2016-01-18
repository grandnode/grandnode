using Nop.Core.Infrastructure;

namespace Nop.Plugin.Feed.Froogle.Data
{
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 0; }
        }
    }
}
