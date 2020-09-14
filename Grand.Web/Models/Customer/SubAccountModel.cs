using Grand.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    public class SubAccountModel : BaseEntityModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
