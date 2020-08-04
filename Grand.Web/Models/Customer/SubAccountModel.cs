using Grand.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    public class SubAccountModel : BaseGrandEntityModel
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
