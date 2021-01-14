using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grand.Admin.Models.Common
{
    public partial class MaintenanceModel : BaseModel
    {
        public MaintenanceModel()
        {
            DeleteGuests = new DeleteGuestsModel();
            DeleteAbandonedCarts = new DeleteAbandonedCartsModel();
            DeleteExportedFiles = new DeleteExportedFilesModel();
            ConvertedPictureModel = new ConvertPictureModel() { NumberOfConvertItems = -1 };
        }

        public DeleteGuestsModel DeleteGuests { get; set; }
        public DeleteAbandonedCartsModel DeleteAbandonedCarts { get; set; }
        public DeleteExportedFilesModel DeleteExportedFiles { get; set; }
        public ConvertPictureModel ConvertedPictureModel { get; set; }

        public bool DeleteActivityLog { get; set; }

        #region Nested classes

        public partial class DeleteGuestsModel : BaseModel
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

        public partial class DeleteAbandonedCartsModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteAbandonedCarts.OlderThan")]
            [UIHint("Date")]
            public DateTime OlderThan { get; set; }

            public int? NumberOfDeletedItems { get; set; }
        }

        public partial class DeleteExportedFilesModel : BaseModel
        {
            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.StartDate")]
            [UIHint("DateNullable")]
            public DateTime? StartDate { get; set; }

            [GrandResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.EndDate")]
            [UIHint("DateNullable")]
            public DateTime? EndDate { get; set; }

            public int? NumberOfDeletedFiles { get; set; }
        }

        public partial class ConvertPictureModel : BaseModel
        {
            public int NumberOfConvertItems { get; set; }
        }

        #endregion
    }
}
