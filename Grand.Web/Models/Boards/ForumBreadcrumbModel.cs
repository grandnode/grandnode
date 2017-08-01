namespace Grand.Web.Models.Boards
{
    public partial class ForumBreadcrumbModel
    {
        public string ForumGroupId { get; set; }
        public string ForumGroupName { get; set; }
        public string ForumGroupSeName { get; set; }
        
        public string ForumId { get; set; }
        public string ForumName { get; set; }
        public string ForumSeName { get; set; }

        public string ForumTopicId { get; set; }
        public string ForumTopicSubject { get; set; }
        public string ForumTopicSeName { get; set; }
    }
}