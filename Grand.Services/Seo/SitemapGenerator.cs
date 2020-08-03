using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.News;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Media;
using Grand.Services.Topics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : ISitemapGenerator
    {
        #region Constants

        private const string DateFormat = @"yyyy-MM-dd";

        /// <summary>
        /// At now each provided sitemap file must have no more than 50000 URLs
        /// </summary>
        private const int maxSitemapUrlNumber = 50000;

        #endregion

        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly IBlogService _blogService;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly CommonSettings _commonSettings;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly NewsSettings _newsSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;

        #endregion

        #region Ctor

        public SitemapGenerator(
            ICategoryService categoryService,
            IProductService productService,
            IManufacturerService manufacturerService,
            ITopicService topicService,
            IBlogService blogService,
            IPictureService pictureService,
            IKnowledgebaseService knowledgebaseService,
            IWebHelper webHelper,
            CommonSettings commonSettings,
            BlogSettings blogSettings,
            KnowledgebaseSettings knowledgebaseSettings,
            NewsSettings newsSettings,
            ForumSettings forumSettings)
        {
            _categoryService = categoryService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _topicService = topicService;
            _blogService = blogService;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _commonSettings = commonSettings;
            _knowledgebaseService = knowledgebaseService;
            _knowledgebaseSettings = knowledgebaseSettings;
            _newsSettings = newsSettings;
            _forumSettings = forumSettings;
            _blogSettings = blogSettings;
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents sitemap URL entry
        /// </summary>
        protected class SitemapUrl
        {
            public SitemapUrl(string location, string image, UpdateFrequency frequency, DateTime updatedOn)
            {
                Location = location;
                Image = image;
                UpdateFrequency = frequency;
                UpdatedOn = updatedOn;
            }

            /// <summary>
            /// Gets or sets URL of the page
            /// </summary>
            public string Location { get; set; }

            /// <summary>
            /// Gets or sets URL of the image
            /// </summary>
            public string Image { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how frequently the page is likely to change
            /// </summary>
            public UpdateFrequency UpdateFrequency { get; set; }

            /// <summary>
            /// Gets or sets the date of last modification of the file
            /// </summary>
            public DateTime UpdatedOn { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get HTTP protocol
        /// </summary>
        /// <returns>Protocol name as string</returns>
        protected virtual string GetHttpProtocol()
        {
            return _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
        }

        /// <summary>
        /// Generate URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>List of URL for the sitemap</returns>
        protected virtual async Task<IList<SitemapUrl>> GenerateUrls(IUrlHelper urlHelper, string language, string store)
        {
            var sitemapUrls = new List<SitemapUrl>();

            //home page
            var homePageUrl = urlHelper.RouteUrl("HomePage", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(homePageUrl, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));

            //search products
            var productSearchUrl = urlHelper.RouteUrl("ProductSearch", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(productSearchUrl, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));

            //contact us
            var contactUsUrl = urlHelper.RouteUrl("ContactUs", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(contactUsUrl, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));

            //news
            if (_newsSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("NewsArchive", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //blog
            if (_blogSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("Blog", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //knowledgebase
            if (_knowledgebaseSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("Knowledgebase", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //forums
            if (_forumSettings.ForumsEnabled)
            {
                var url = urlHelper.RouteUrl("Boards", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //categories
            if (_commonSettings.SitemapIncludeCategories)
                sitemapUrls.AddRange(await GetCategoryUrls(urlHelper, "", language));

            //manufacturers
            if (_commonSettings.SitemapIncludeManufacturers)
                sitemapUrls.AddRange(await GetManufacturerUrls(urlHelper, language, store));

            //products
            if (_commonSettings.SitemapIncludeProducts)
                sitemapUrls.AddRange(await GetProductUrls(urlHelper, language, store));
            
            //topics
            sitemapUrls.AddRange(await GetTopicUrls(urlHelper, language, store));

            //blog posts
            sitemapUrls.AddRange(await GetBlogPostsUrls(urlHelper, language, store));

            //knowledgebase articles
            sitemapUrls.AddRange(await GetKnowledgebaseUrls(urlHelper, language));

            //custom URLs
            sitemapUrls.AddRange(GetCustomUrls());

            return sitemapUrls;
        }

        /// <summary>
        /// Get category URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="sitemapUrls">Current list of URL</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetCategoryUrls(IUrlHelper urlHelper, string parentCategoryId, string language)
        {
            var allCategoriesByParentCategoryId = await _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId: parentCategoryId);
            var categories = new List<SitemapUrl>();
            var storeLocation = _webHelper.GetStoreLocation();
            foreach (var category in allCategoriesByParentCategoryId)
            {
                var url = urlHelper.RouteUrl("Category", new { SeName = category.GetSeName(language) }, GetHttpProtocol());
                var imageurl = string.Empty;
                if(_commonSettings.SitemapIncludeImage)
                {
                    if(!string.IsNullOrEmpty(category.PictureId))
                    {
                        imageurl = await _pictureService.GetPictureUrl(category.PictureId, showDefaultPicture: false, storeLocation: storeLocation);
                    }
                }
                categories.Add(new SitemapUrl(url, imageurl, UpdateFrequency.Weekly, category.UpdatedOnUtc));
                categories.AddRange(await GetCategoryUrls(urlHelper, category.Id, language));
            }
            return categories;
        }

        /// <summary>
        /// Get manufacturer URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetManufacturerUrls(IUrlHelper urlHelper, string language, string store)
        {
            var manuf = await _manufacturerService.GetAllManufacturers(storeId: store);
            var manufactures = new List<SitemapUrl>();
            var storeLocation = _webHelper.GetStoreLocation();
            foreach (var manufacturer in manuf)
            {
                var url = urlHelper.RouteUrl("Manufacturer", new { SeName = manufacturer.GetSeName(language) }, GetHttpProtocol());
                var imageurl = string.Empty;
                if (_commonSettings.SitemapIncludeImage)
                {
                    if (!string.IsNullOrEmpty(manufacturer.PictureId))
                    {
                        imageurl = await _pictureService.GetPictureUrl(manufacturer.PictureId, showDefaultPicture: false, storeLocation: storeLocation);
                    }
                }
                manufactures.Add(new SitemapUrl(url, imageurl, UpdateFrequency.Weekly, manufacturer.UpdatedOnUtc));
            }
            return manufactures;
        }

        /// <summary>
        /// Get product URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetProductUrls(IUrlHelper urlHelper, string language, string store)
        {
            var search = await _productService.SearchProducts(storeId: store,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn);
            var storeLocation = _webHelper.GetStoreLocation();
            var products = new List<SitemapUrl>();
            foreach (var product in search.products)
            {
                var url = urlHelper.RouteUrl("Product", new { SeName = product.GetSeName(language) }, GetHttpProtocol());
                var imageurl = string.Empty;
                if (_commonSettings.SitemapIncludeImage)
                {
                    if (!string.IsNullOrEmpty(product.ProductPictures.FirstOrDefault()?.PictureId))
                    {
                        imageurl = await _pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().PictureId, showDefaultPicture: false, storeLocation: storeLocation);
                    }
                }
                products.Add(new SitemapUrl(url, imageurl, UpdateFrequency.Weekly, product.UpdatedOnUtc));
            }
            return products;
            
        }

        /// <summary>
        /// Get topic URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetTopicUrls(IUrlHelper urlHelper, string language, string store)
        {
            var topics = await _topicService.GetAllTopics(storeId: store);
            return topics.Where(t => t.IncludeInSitemap).Select(topic =>
            {
                var url = urlHelper.RouteUrl("Topic", new { SeName = topic.GetSeName(language) }, GetHttpProtocol());
                return new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow);
            });
        }

        /// <summary>
        /// Get blog posts URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetBlogPostsUrls(IUrlHelper urlHelper, string language, string store)
        {
            var blogposts = await _blogService.GetAllBlogPosts(storeId: store);
            var blog = new List<SitemapUrl>();
            var storeLocation = _webHelper.GetStoreLocation();
            foreach (var blogpost in blogposts)
            {
                var url = urlHelper.RouteUrl("BlogPost", new { SeName = blogpost.GetSeName(language) }, GetHttpProtocol());
                var imageurl = string.Empty;
                if (_commonSettings.SitemapIncludeImage)
                {
                    if (!string.IsNullOrEmpty(blogpost.PictureId))
                    {
                        imageurl = await _pictureService.GetPictureUrl(blogpost.PictureId, showDefaultPicture: false, storeLocation: storeLocation);
                    }
                }
                blog.Add(new SitemapUrl(url, imageurl, UpdateFrequency.Weekly, DateTime.UtcNow));
            }
            
            return blog;
        }

        /// <summary>
        /// Get knowledgebase articles URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual async Task<IEnumerable<SitemapUrl>> GetKnowledgebaseUrls(IUrlHelper urlHelper, string language)
        {
            var knowledgebasearticles = await _knowledgebaseService.GetPublicKnowledgebaseArticles();
            var knowledgebase = new List<SitemapUrl>();
            foreach (var knowledgebasearticle in knowledgebasearticles)
            {
                var url = urlHelper.RouteUrl("KnowledgebaseArticle", new { SeName = knowledgebasearticle.GetSeName(language) }, GetHttpProtocol());
                knowledgebase.Add(new SitemapUrl(url, string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            return knowledgebase;
        }

        /// <summary>
        /// Get custom URLs for the sitemap
        /// </summary>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetCustomUrls()
        {
            var storeLocation = _webHelper.GetStoreLocation();

            return _commonSettings.SitemapCustomUrls.Select(customUrl =>
                new SitemapUrl(string.Concat(storeLocation, customUrl), string.Empty, UpdateFrequency.Weekly, DateTime.UtcNow));
        }

        /// <summary>
        /// Write sitemap index file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapNumber">The number of sitemaps</param>
        protected virtual async Task WriteSitemapIndex(IUrlHelper urlHelper, Stream stream, int sitemapNumber)
        {
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Indent = true,
                IndentChars = "\t",
                NewLineChars = "\r\n",
                Encoding = Encoding.UTF8,
                Async = true
            };

            using (var writer = XmlWriter.Create(stream, xwSettings))
            {
                await writer.WriteStartDocumentAsync();
                writer.WriteStartElement("sitemapindex");
                writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                //write URLs of all available sitemaps
                for (var id = 1; id <= sitemapNumber; id++)
                {
                    var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, GetHttpProtocol());
                    var location = XmlHelper.XmlEncode(url);

                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", location);
                    writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(DateFormat));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// Write sitemap file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapUrls">List of sitemap URLs</param>
        protected virtual async Task WriteSitemap(IUrlHelper urlHelper, Stream stream, IList<SitemapUrl> sitemapUrls)
        {
            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                Indent = true,
                IndentChars = "\t",
                NewLineChars = "\r\n",
                Encoding = Encoding.UTF8,
                Async = true
            };

            using (var writer = XmlWriter.Create(stream, xwSettings))
            {
                await writer.WriteStartDocumentAsync();
                writer.WriteStartElement("urlset");
                await writer.WriteAttributeStringAsync("urlset", "xmlns", null, "http://www.sitemaps.org/schemas/sitemap/0.9");

                if (_commonSettings.SitemapIncludeImage)
                    await writer.WriteAttributeStringAsync("xmlns", "image", null, "http://www.google.com/schemas/sitemap-image/1.1");

                await writer.WriteAttributeStringAsync("xsi", "schemaLocation", null, "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                //write URLs from list to the sitemap
                foreach (var url in sitemapUrls)
                {
                    writer.WriteStartElement("url");
                    var location = XmlHelper.XmlEncode(url.Location);
                    writer.WriteElementString("loc", location);

                    if (_commonSettings.SitemapIncludeImage && !string.IsNullOrEmpty(url.Image))
                    {
                        writer.WriteStartElement("image", "image", null);
                        writer.WriteElementString("image", "loc", null, url.Image);
                        writer.WriteEndElement();
                    }

                    writer.WriteElementString("changefreq", url.UpdateFrequency.ToString().ToLowerInvariant());
                    writer.WriteElementString("lastmod", url.UpdatedOn.ToString(DateFormat));
                    writer.WriteEndElement();
                }

                await writer.WriteEndElementAsync();
                await writer.FlushAsync();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="language">Lang ident</param>
        /// <param name="store">Store ident</param>
        /// <returns>Sitemap.xml as string</returns>
        public virtual async Task<string> Generate(IUrlHelper urlHelper, int? id, string language, string store)
        {
            using (var stream = new MemoryStream())
            {
                await Generate(urlHelper, stream, id, language, store);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream of sitemap.</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="language">Lang ident</param>
        /// <param name="store">Store ident</param>
        public virtual async Task Generate(IUrlHelper urlHelper, Stream stream, int? id, string language, string store)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = await GenerateUrls(urlHelper, language, store);

            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls.Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / maxSitemapUrlNumber).Select(group => group.Select(url => url.Value).ToList()).ToList();

            if (!sitemaps.Any())
                return;

            if (id.HasValue)
            {
                //requested sitemap does not exist
                if (id.Value == 0 || id.Value > sitemaps.Count)
                    return;

                //otherwise write a certain numbered sitemap file into the stream
                await WriteSitemap(urlHelper, stream, sitemaps.ElementAt(id.Value - 1));

            }
            else
            {
                //URLs more than the maximum allowable, so generate a sitemap index file
                if (sitemapUrls.Count >= maxSitemapUrlNumber)
                {
                    //write a sitemap index file into the stream
                    await WriteSitemapIndex(urlHelper, stream, sitemaps.Count);
                }
                else
                {
                    //otherwise generate a standard sitemap
                    await WriteSitemap(urlHelper, stream, sitemaps.First());
                }
            }
        }

        #endregion
    }
}