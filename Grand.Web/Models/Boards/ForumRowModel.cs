
namespace Grand.Web.Models.Boards
{
    public partial class ForumRowModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Description { get; set; }
        public int NumTopics { get; set; }
        public int NumPosts { get; set; }
        public string LastPostId { get; set; }
    }
}