using System.Threading.Tasks;

namespace Grand.Services.Installation
{
    public partial interface IUpgradeService
    {
        string DatabaseVersion();
        Task UpgradeData(string fromversion, string toversion);
    }
}
