using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Install
{
    public partial class InstallModel : BaseGrandModel
    {
        public InstallModel()
        {
            AvailableLanguages = new List<SelectListItem>();
            AvailableCollation = new List<SelectListItem>();
        }
        [DataType(DataType.EmailAddress)]
        public string AdminEmail { get; set; }
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DataProvider { get; set; }
        public bool MongoDBConnectionInfo { get; set; }
        public string MongoDBServerName { get; set; }
        public string MongoDBDatabaseName { get; set; }
        public string MongoDBUsername { get; set; }
        [DataType(DataType.Password)]
        public string MongoDBPassword { get; set; }
        public bool DisableSampleDataOption { get; set; }
        public bool InstallSampleData { get; set; }
        public bool Installed { get; set; }
        public string Collation { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }
        public List<SelectListItem> AvailableCollation { get; set; }
    }
}