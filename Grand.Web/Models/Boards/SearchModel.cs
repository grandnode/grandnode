﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.Boards
{
    public partial class SearchModel
    {
        public SearchModel()
        {
            LimitList = new List<SelectListItem>();
            ForumList = new List<SelectListItem>();
            WithinList = new List<SelectListItem>();
            ForumTopics = new List<ForumTopicRowModel>();
        }

        public bool ShowAdvancedSearch { get; set; }

        public string SearchTerms { get; set; }

        public int? ForumId { get; set; }

        public int? Within { get; set; }

        public int? LimitDays { get; set; }

        public IList<ForumTopicRowModel> ForumTopics { get; set; }
        public int TopicPageSize { get; set; }
        public int TopicTotalRecords { get; set; }
        public int TopicPageIndex { get; set; }

        public List<SelectListItem> LimitList { get; set; }

        public List<SelectListItem> ForumList { get; set; }

        public List<SelectListItem> WithinList { get; set; }

        public string ForumIdSelected { get; set; }

        public int WithinSelected { get; set; }

        public int LimitDaysSelected { get; set; }

        public bool SearchResultsVisible { get; set; }

        public bool NoResultsVisisble { get; set; }

        public string Error { get; set; }

        public int PostsPageSize { get; set; }
        public bool AllowPostVoting { get; set; }
    }
}