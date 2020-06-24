using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Grand.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial interface ISitemapGenerator
    {
        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="language">Lang ident</param>
        /// <param name="store">Store ident</param>
        /// <returns>Sitemap.xml as string</returns>
        Task<string> Generate(IUrlHelper urlHelper, int? id, string language, string store);

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream of sitemap.</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="language">Lang ident</param>
        /// <param name="store">Store ident</param>
        Task Generate(IUrlHelper urlHelper, Stream stream, int? id, string language, string store);
    }
}
