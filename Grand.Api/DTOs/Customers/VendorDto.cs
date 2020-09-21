using Grand.Api.Models;

namespace Grand.Api.DTOs.Customers
{
    public partial class VendorDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public string PictureId { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string StoreId { get; set; }
        public string AdminComment { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int DisplayOrder { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public virtual AddressDto Address { get; set; }
    }
}
