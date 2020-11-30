// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Threading.Tasks;

namespace Grand.Framework.Components
{
    /// <summary>
    /// An <see cref="IViewComponentResult"/> which writes an <see cref="IHtmlContent"/> when executed.
    /// </summary>
    /// <remarks>
    /// The provided content will be HTML-encoded as specified when the content was created. To encoded and write
    /// text, use a <see cref="ContentViewComponentResult"/>.
    /// </remarks>
    public class JsonContentViewComponentResult : IViewComponentResult
    {
        /// <summary>
        /// Initializes a new <see cref="HtmlContentViewComponentResult"/>.
        /// </summary>
        public JsonContentViewComponentResult(IHtmlContent encodedContent)
        {
            EncodedContent = encodedContent ?? throw new ArgumentNullException(nameof(encodedContent));
        }

        /// <summary>
        /// Gets the encoded content.
        /// </summary>
        public IHtmlContent EncodedContent { get; }

        /// <summary>
        /// Writes the <see cref="EncodedContent"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/>.</param>
        public void Execute(ViewComponentContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ViewContext.HttpContext.Response.ContentType = "application/json";
            context.Writer.Write(EncodedContent);
        }

        /// <summary>
        /// Writes the <see cref="EncodedContent"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/>.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task ExecuteAsync(ViewComponentContext context)
        {
            Execute(context);

            return Task.CompletedTask;
        }
    }
}
