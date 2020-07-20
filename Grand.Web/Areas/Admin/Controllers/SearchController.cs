using Grand.Core;
using Grand.Domain;
using Grand.Domain.AdminSearch;
using Grand.Domain.Customers;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.News;
using Grand.Services.Orders;
using Grand.Services.Topics;
using Grand.Web.Areas.Admin.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class SearchController : BaseAdminController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly INewsService _newsService;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly AdminSearchSettings _adminSearchSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        public SearchController(IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService,
            ITopicService topicService, INewsService newsService, IBlogService blogService, ICustomerService customerService, IOrderService orderService,
            AdminSearchSettings adminSearchSettings, ILocalizationService localizationService, IWorkContext workContext)
        {
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _topicService = topicService;
            _newsService = newsService;
            _blogService = blogService;
            _customerService = customerService;
            _orderService = orderService;
            _adminSearchSettings = adminSearchSettings;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchTerm, FoundMenuItem[] foundMenuItems)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json("error");

            if (!_workContext.CurrentCustomer.IsAdmin())
            {
                return Json("Access Denied");
            }
            //object = actual result, int = display order for sorting
            List<Tuple<object, int>> result = new List<Tuple<object, int>>();

            if (searchTerm.Length >= _adminSearchSettings.MinSearchTermLength)
            {
                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInProducts)
                {
                    var products = (await _productService.SearchProducts(keywords: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count(), showHidden: true)).products;
                    foreach (var product in products)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = product.Name,
                            link = Url.Content("~/Admin/Product/Edit/") + product.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Products")
                        }, _adminSearchSettings.ProductsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInCategories)
                {
                    var categories = await _categoryService.GetAllCategories(searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count(), showHidden: _workContext.CurrentCustomer.IsAdmin());
                    foreach (var category in categories)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = category.Name,
                            link = Url.Content("~/Admin/Category/Edit/") + category.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Categories")
                        }, _adminSearchSettings.CategoriesDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInManufacturers)
                {
                    var manufacturers = await _manufacturerService.GetAllManufacturers(searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count(), showHidden: true);
                    foreach (var manufacturer in manufacturers)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = manufacturer.Name,
                            link = Url.Content("~/Admin/Manufacturer/Edit/") + manufacturer.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Manufacturers")
                        }, _adminSearchSettings.ManufacturersDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInTopics)
                {
                    var topics = (await _topicService.GetAllTopics("")).Where(x => (x.SystemName != null && x.SystemName.ToLower().Contains(searchTerm.ToLower())) ||
                    (x.Title != null && x.Title.ToLower().Contains(searchTerm.ToLower())));
                    foreach (var topic in topics)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = topic.SystemName,
                            link = Url.Content("~/Admin/Topic/Edit/") + topic.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.Topics")
                        }, _adminSearchSettings.TopicsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInNews)
                {
                    var news = await _newsService.GetAllNews(newsTitle: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count(), showHidden: true);
                    foreach (var signleNews in news)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = signleNews.Title,
                            link = Url.Content("~/Admin/News/Edit/") + signleNews.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.News")
                        }, _adminSearchSettings.NewsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInBlogs)
                {
                    var blogPosts = await _blogService.GetAllBlogPosts(blogPostName: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count(), showHidden: true);
                    foreach (var blogPost in blogPosts)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = blogPost.Title,
                            link = Url.Content("~/Admin/Blog/Edit/") + blogPost.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.Blog")
                        }, _adminSearchSettings.BlogsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInCustomers)
                {
                    var customersByEmail = await _customerService.GetAllCustomers(email: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    IPagedList<Customer> customersByUsername = new PagedList<Customer>();
                    if (_adminSearchSettings.MaxSearchResultsCount - result.Count() - customersByEmail.Count() > 0)
                    {
                        customersByUsername = await _customerService.GetAllCustomers(username: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount
                            - result.Count() - customersByEmail.Count());
                    }
                    var combined = customersByEmail.Union(customersByUsername).GroupBy(x => x.Email).Select(x => x.First());

                    foreach (var customer in combined)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = customer.Email,
                            link = Url.Content("~/Admin/Customer/Edit/") + customer.Id,
                            source = _localizationService.GetResource("Admin.Customers")
                        }, _adminSearchSettings.CustomersDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInMenu && foundMenuItems != null && foundMenuItems.Any())
                {
                    foreach (var menuItem in foundMenuItems)
                    {
                        if (result.Count() >= _adminSearchSettings.MaxSearchResultsCount)
                            break;

                        string formatted = _localizationService.GetResource("Admin.AdminSearch.Menu") + " > ";
                        if (string.IsNullOrEmpty(menuItem.grandParent))
                        {
                            formatted += menuItem.parent;
                        }
                        else
                        {
                            formatted += menuItem.grandParent + " > " + menuItem.parent;
                        }

                        result.Add(new Tuple<object, int>(new
                        {
                            title = menuItem.title,
                            link = menuItem.link,
                            source = formatted
                        }, _adminSearchSettings.MenuDisplayOrder));
                    }
                }
            }

            if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInOrders)
            {
                int.TryParse(searchTerm, out int orderNumber);
                if (orderNumber > 0)
                {
                    var order = await _orderService.GetOrderByNumber(orderNumber);
                    if (order != null)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = order.OrderNumber,
                            link = Url.Content("~/Admin/Order/Edit/") + order.Id,
                            source = _localizationService.GetResource("Admin.Orders")
                        }, _adminSearchSettings.OrdersDisplayOrder));
                    }
                }
                var orders = await _orderService.GetOrdersByCode(searchTerm);
                foreach (var order in orders)
                {
                    result.Add(new Tuple<object, int>(new
                    {
                        title = order.Code,
                        link = Url.Content("~/Admin/Order/Edit/") + order.Id,
                        source = _localizationService.GetResource("Admin.Orders")
                    }, _adminSearchSettings.OrdersDisplayOrder));
                }

            }

            result = result.OrderBy(x => x.Item2).ToList();
            return Json(result.Take(_adminSearchSettings.MaxSearchResultsCount).Select(x => x.Item1).ToList());
        }
    }
}