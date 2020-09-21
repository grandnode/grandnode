using Grand.Core;
using Grand.Core.Data;
using Grand.Services.Installation;
using Grand.Web.Models.Upgrade;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class UpgradeController : BasePublicController
    {
        #region Fields

        private readonly IUpgradeService _upgradeService;

        #endregion

        #region Ctor

        public UpgradeController(IUpgradeService upgradeService)
        {
            _upgradeService = upgradeService;
        }
        #endregion

        public virtual IActionResult Index()
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("Install");

            var model = new UpgradeModel {
                ApplicationVersion = GrandVersion.FullVersion,
                ApplicationDBVersion = GrandVersion.SupportedDBVersion,
                DatabaseVersion = _upgradeService.DatabaseVersion()
            };

            if (model.ApplicationDBVersion == model.DatabaseVersion)
                return RedirectToRoute("Homepage");

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Index(UpgradeModel m, [FromServices] IWebHelper webHelper)
        {
            var model = new UpgradeModel {
                ApplicationDBVersion = GrandVersion.SupportedDBVersion,
                DatabaseVersion = _upgradeService.DatabaseVersion()
            };

            if (model.ApplicationDBVersion != model.DatabaseVersion)
            {
                await _upgradeService.UpgradeData(model.DatabaseVersion, model.ApplicationDBVersion);
            }
            else
                return RedirectToRoute("HomePage");

            //restart application
            webHelper.RestartAppDomain();

            //Redirect to home page
            return RedirectToRoute("HomePage");

        }
    }
}