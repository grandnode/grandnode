
namespace Grand.Core.Data
{
    /// <summary>
    /// Data provider interface
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Initialize database
        /// </summary>
        void InitDatabase();

        /// <summary>
        /// Set database initializer
        /// </summary>
        void SetDatabaseInitializer();

    }
}
