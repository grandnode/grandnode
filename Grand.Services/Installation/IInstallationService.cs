namespace Grand.Services.Installation
{
    public partial interface IInstallationService
    {
        void InstallData(string defaultUserEmail, string defaultUserPassword, string collation, bool installSampleData = true);
    }
}
