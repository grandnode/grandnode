using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.News;
using Grand.Services.Topics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class AdminSearchController : BaseAdminController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly INewsService _newsService;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;

        public AdminSearchController(IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService,
            ITopicService topicService, INewsService newsService, IBlogService blogService, ICustomerService customerService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._newsService = newsService;
            this._blogService = blogService;
            this._customerService = customerService;
        }

        [HttpPost]
        public IActionResult Search(string searchTerm)
        {
            List<object> result = new List<object>();

            var products = _productService.SearchProducts(keywords: searchTerm);
            foreach (var product in products)
            {
                result.Add(new
                {
                    title = product.Name,
                    link = Url.Content("~/Admin/Product/Edit/") + product.Id,
                    source = "Products"
                });
            }

            var categories = _categoryService.GetAllCategories(searchTerm);
            foreach (var category in categories)
            {
                result.Add(new
                {
                    title = category.Name,
                    link = Url.Content("~/Admin/Category/Edit/") + category.Id,
                    source = "Categories"
                });
            }

            var manufacturers = _manufacturerService.GetAllManufacturers(searchTerm);
            foreach (var manufacturer in manufacturers)
            {
                result.Add(new
                {
                    title = manufacturer.Name,
                    link = Url.Content("~/Admin/Manufacturer/Edit/") + manufacturer.Id,
                    source = "Manufacturers"
                });
            }

            var topics = _topicService.GetAllTopics("", topicSystemName: searchTerm);
            foreach (var topic in topics)
            {
                result.Add(new
                {
                    title = topic.SystemName,
                    link = Url.Content("~/Admin/Topic/Edit/") + topic.Id,
                    source = "Topics"
                });
            }

            var news = _newsService.GetAllNews(newsTitle: searchTerm);
            foreach (var signleNews in news)
            {
                result.Add(new
                {
                    title = signleNews.Title,
                    link = Url.Content("~/Admin/News/Edit/") + signleNews.Id,
                    source = "News"
                });
            }

            var blogPosts = _blogService.GetAllBlogPosts(blogPostName: searchTerm);
            foreach (var blogPost in blogPosts)
            {
                result.Add(new
                {
                    title = blogPost.Title,
                    link = Url.Content("~/Admin/Blog/Edit/") + blogPost.Id,
                    source = "Blog posts"
                });
            }

            var customers = _customerService.getcus//name and username?

            return Json(result);
        }
    }
}
