using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Customer
{
    public class SubAccountModel : BaseGrandModel
    {
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }
    }
}
