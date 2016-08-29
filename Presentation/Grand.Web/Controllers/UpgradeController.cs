﻿using Grand.Core;
using Grand.Core.Data;
using Grand.Services.Installation;
using Grand.Web.Models.Upgrade;
using System.Web.Mvc;

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
            this._upgradeService = upgradeService;
        }
        #endregion

        public ActionResult Index()
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("Install");

            var model = new UpgradeModel();
            model.ApplicationVersion = GrandVersion.CurrentVersion;
            model.DatabaseVersion = _upgradeService.DatabaseVersion();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(UpgradeModel m)
        {
            var model = new UpgradeModel();
            model.ApplicationVersion = GrandVersion.CurrentVersion;
            model.DatabaseVersion = _upgradeService.DatabaseVersion();

            if (model.ApplicationVersion != model.DatabaseVersion)
            {
                _upgradeService.UpgradeData(model.DatabaseVersion, model.ApplicationVersion);
            }
            model.ApplicationVersion = GrandVersion.CurrentVersion;
            model.DatabaseVersion = _upgradeService.DatabaseVersion();
            return View(model);
        }
    }
}