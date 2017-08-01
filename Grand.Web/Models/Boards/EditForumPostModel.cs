using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Core.Domain.Forums;
using Grand.Web.Validators.Boards;

namespace Grand.Web.Models.Boards
{
    [Validator(typeof(EditForumPostValidator))]
    public partial class EditForumPostModel
    {
        public string Id { get; set; }
        public string ForumTopicId { get; set; }

        public bool IsEdit { get; set; }

        public string Text { get; set; }
        public EditorType ForumEditor { get; set; }

        public string ForumName { get; set; }
        public string ForumTopicSubject { get; set; }
        public string ForumTopicSeName { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }
        public bool Subscribed { get; set; }
    }
}