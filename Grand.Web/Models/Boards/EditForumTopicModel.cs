using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Core.Domain.Forums;
using Grand.Web.Validators.Boards;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Models.Boards
{
    [Validator(typeof(EditForumTopicValidator))]
    public partial class EditForumTopicModel
    {
        public EditForumTopicModel()
        {
            TopicPriorities = new List<SelectListItem>();
        }

        public bool IsEdit { get; set; }

        public string Id { get; set; }

        public string ForumId { get; set; }
        public string ForumName { get; set; }
        public string ForumSeName { get; set; }

        public int TopicTypeId { get; set; }
        public EditorType ForumEditor { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        
        public bool IsCustomerAllowedToSetTopicPriority { get; set; }
        public IEnumerable<SelectListItem> TopicPriorities { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }
        public bool Subscribed { get; set; }

    }
}