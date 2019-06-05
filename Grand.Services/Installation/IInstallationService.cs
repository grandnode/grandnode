using System.Threading.Tasks;

namespace Grand.Services.Installation
{
    public partial interface IInstallationService
    {
        Task InstallData(string defaultUserEmail, string defaultUserPassword, string collation = "en", bool installSampleData = true);
    }
}
