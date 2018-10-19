namespace Grand.Framework.Mvc.Models
{
    public class DeleteConfirmationModel : BaseGrandEntityModel
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string WindowId { get; set; }
    }
}