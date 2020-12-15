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
        public JsonContentViewComponentResult(string content)
        {
            EncodedContent = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// Gets the encoded content.
        /// </summary>
        public string EncodedContent { get; }

        /// <summary>
        /// Set content type for reponse
        /// </summary>
        /// <param name="context"></param>
        protected void ContentType(ViewComponentContext context)
        {
            context.ViewContext.HttpContext.Response.ContentType = "application/json";
        }

        public void Execute(ViewComponentContext context)
        {
            ContentType(context);
            context.Writer.Write(EncodedContent);
        }

        /// <summary>
        /// Writes the <see cref="EncodedContent"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/>.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public async Task ExecuteAsync(ViewComponentContext context)
        {
            ContentType(context);
            await context.Writer.WriteAsync(EncodedContent);
        }
    }
}