namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class GenericAttributeModel
    {
        public string Id { get; set; }
        public string ObjectType { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string StoreId { get; set; }
        public int SelectedTab { get; set; }
    }
}