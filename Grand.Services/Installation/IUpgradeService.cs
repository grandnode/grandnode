namespace Grand.Services.Installation
{
    public partial interface IUpgradeService
    {
        string DatabaseVersion();
        void UpgradeData(string fromversion, string toversion);
    }
}
