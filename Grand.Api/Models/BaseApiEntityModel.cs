using System.ComponentModel.DataAnnotations;

namespace Grand.Api.Models
{
    public partial class BaseApiEntityModel
    {
        [Key]
        public string Id { get; set; }
    }
}
