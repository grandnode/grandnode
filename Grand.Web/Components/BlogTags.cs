﻿using Grand.Core.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Features.Models.Blogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class BlogTagsViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly BlogSettings _blogSettings;

        public BlogTagsViewComponent(IMediator mediator, BlogSettings blogSettings)
        {
            _mediator = mediator;
            _blogSettings = blogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = await _mediator.Send(new GetBlogPostTagList());
            return View(model);

        }
    }
}