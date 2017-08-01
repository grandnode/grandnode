using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class MaintenanceModel : BaseGrandModel
    {
        public MaintenanceModel()
        {
            DeleteGuests = new DeleteGuestsModel();
            DeleteAbandonedCarts = new DeleteAbandonedCartsModel();
            DeleteExportedFiles = new DeleteExportedFilesModel();
        }

        public DeleteGuestsModel DeleteGuests { get; set; }
        public DeleteAbandonedCartsModel DeleteAbandonedCarts { get; set; }
        public DeleteExportedFilesModel DeleteExportedFiles { get; set; }
        public bool DeleteActivityLog { get; set; }

        #region Nested classes

        public partial class DeleteGuestsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteGuests.StartDate")]
            [UIHint("DateNullable")]
            public DateTime? StartDate { get; set; }

            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteGuests.EndDate")]
            [UIHint("DateNullable")]
            public DateTime? EndDate { get; set; }

            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteGuests.OnlyWithoutShoppingCart")]
            public bool OnlyWithoutShoppingCart { get; set; }

            public int? NumberOfDeletedCustomers { get; set; }
        }

        public partial class DeleteAbandonedCartsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteAbandonedCarts.OlderThan")]
            [UIHint("Date")]
            public DateTime OlderThan { get; set; }

            public int? NumberOfDeletedItems { get; set; }
        }

        public partial class DeleteExportedFilesModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.StartDate")]
            [UIHint("DateNullable")]
            public DateTime? StartDate { get; set; }

            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.EndDate")]
            [UIHint("DateNullable")]
            public DateTime? EndDate { get; set; }

            public int? NumberOfDeletedFiles { get; set; }
        }

        #endregion
    }
}
