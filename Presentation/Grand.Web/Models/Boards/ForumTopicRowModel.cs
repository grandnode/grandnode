using Grand.Core.Domain.Forums;

namespace Grand.Web.Models.Boards
{
    public partial class ForumTopicRowModel
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string SeName { get; set; }
        public string LastPostId { get; set; }

        public int NumPosts { get; set; }
        public int Views { get; set; }
        public int NumReplies { get; set; }
        public ForumTopicType ForumTopicType { get; set; }

        public string CustomerId { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerGuest { get; set; }

        //posts
        public int TotalPostPages { get; set; }
        public int Votes { get; set; }
    }
}