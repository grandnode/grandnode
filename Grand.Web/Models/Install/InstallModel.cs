using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Install;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Install
{
    [Validator(typeof(InstallValidator))]
    public partial class InstallModel : BaseGrandModel
    {
        public InstallModel()
        {
            this.AvailableLanguages = new List<SelectListItem>();
            this.AvailableCollation = new List<SelectListItem>();
        }
        public string AdminEmail { get; set; }
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DataProvider { get; set; }
        public bool UseConnectionString { get; set; }
        public string MongoDBServerName { get; set; }

        public System.Security.Authentication.SslProtocols SslProtocol { get; set; } = System.Security.Authentication.SslProtocols.Tls12;

        public int MongoDBServerPort { get; set; } = 27017;

        public string MongoCredentialMechanism { get; set; } = "SCRAM-SHA-1";

        public string ReplicaSet { get; set; } = "globaldb";

        public string MongoDBDatabaseName { get; set; }
        public string MongoDBUsername { get; set; }
        [DataType(DataType.Password)]
        public string MongoDBPassword { get; set; }
        public bool DisableSampleDataOption { get; set; }
        public bool InstallSampleData { get; set; } = true;
        public bool Installed { get; set; }
        public string Collation { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }
        public List<SelectListItem> AvailableCollation { get; set; }
        public List<SelectListItem> SslProtocols { get; set; }

    }
}