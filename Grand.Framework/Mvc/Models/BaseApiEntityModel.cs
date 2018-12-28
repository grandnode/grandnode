using System.ComponentModel.DataAnnotations;

namespace Grand.Framework.Mvc.Models
{
    public partial class BaseApiEntityModel
    {
        [Key]
        public string Id { get; set; }
    }
}
