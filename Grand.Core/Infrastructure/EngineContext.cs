using System.Runtime.CompilerServices;

namespace Grand.Core.Infrastructure
{
    /// <summary>
    /// Provides access to the singleton instance of the Grand engine.
    /// </summary>
    public static class EngineContext
    {
        #region Methods

        /// <summary>
        /// Create a static instance of the Grand engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        public static IEngine Create()
        {
            //create GrandEngine as engine
            if (Singleton<IEngine>.Instance == null)
                Singleton<IEngine>.Instance = new GrandEngine();

            return Singleton<IEngine>.Instance;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton Grand engine used to access Nop services.
        /// </summary>
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Create();
                }

                return Singleton<IEngine>.Instance;
            }
        }

        #endregion
    }
}
