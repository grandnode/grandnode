
namespace Grand.Domain.Data
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
        /// Get connetction string
        /// </summary>
        public string ConnectionString { get; }
    }
}
