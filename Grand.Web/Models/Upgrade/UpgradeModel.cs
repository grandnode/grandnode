using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Models.Upgrade
{
    public partial class UpgradeModel
    {
        public string DatabaseVersion { get; set; }
        public string ApplicationVersion { get; set; }
    }
}