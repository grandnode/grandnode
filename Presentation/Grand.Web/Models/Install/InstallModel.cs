using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Install;

namespace Grand.Web.Models.Install
{
    [Validator(typeof(InstallValidator))]
    public partial class InstallModel : BaseNopModel
    {
        public InstallModel()
        {
            this.AvailableLanguages = new List<SelectListItem>();
        }

        [AllowHtml]
        public string AdminEmail { get; set; }

        [AllowHtml]
        [NoTrim]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        [AllowHtml]
        [NoTrim]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


        [AllowHtml]
        public string DatabaseConnectionString { get; set; }

        public string DataProvider { get; set; }

        public bool MongoDBConnectionInfo { get; set; }
        [AllowHtml]
        public string MongoDBServerName { get; set; }
        [AllowHtml]
        public string MongoDBDatabaseName { get; set; }
        [AllowHtml]
        public string MongoDBUsername { get; set; }
        [AllowHtml]
        public string MongoDBPassword { get; set; }
        public bool DisableSampleDataOption { get; set; }
        public bool InstallSampleData { get; set; }

        public List<SelectListItem> AvailableLanguages { get; set; }
    }
}