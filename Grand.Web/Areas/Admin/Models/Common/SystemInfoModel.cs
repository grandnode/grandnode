﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class SystemInfoModel : BaseModel
    {
        public SystemInfoModel()
        {
            this.ServerVariables = new List<ServerVariableModel>();
            this.LoadedAssemblies = new List<LoadedAssembly>();
        }

        [GrandResourceDisplayName("Admin.System.SystemInfo.ASPNETInfo")]
        public string AspNetInfo { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.MachineName")]
        public string MachineName { get; set; }


        [GrandResourceDisplayName("Admin.System.SystemInfo.GrandVersion")]
        public string GrandVersion { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.OperatingSystem")]
        public string OperatingSystem { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.ServerLocalTime")]
        public DateTime ServerLocalTime { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.ApplicationTime")]
        public DateTime ApplicationTime { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.ServerTimeZone")]
        public string ServerTimeZone { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.UTCTime")]
        public DateTime UtcTime { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.Scheme")]
        public string RequestScheme { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.IsHttps")]
        public bool IsHttps { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.ServerVariables")]
        public IList<ServerVariableModel> ServerVariables { get; set; }

        [GrandResourceDisplayName("Admin.System.SystemInfo.LoadedAssemblies")]
        public IList<LoadedAssembly> LoadedAssemblies { get; set; }

        public partial class ServerVariableModel : BaseModel
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public partial class LoadedAssembly : BaseModel
        {
            public string FullName { get; set; }
            public string Location { get; set; }
        }
    }
}