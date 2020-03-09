﻿using Grand.Core.Domain.News;
using Grand.Web.Models.News;
using MediatR;

namespace Grand.Web.Commands.Models
{
    public class InsertNewsCommentCommandModel : IRequest<NewsComment>
    {
        public NewsItem NewsItem { get; set; }
        public NewsItemModel Model { get; set; }
    }
}
