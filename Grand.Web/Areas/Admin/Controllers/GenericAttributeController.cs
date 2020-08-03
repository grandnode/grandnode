using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Customers;
using Grand.Framework.Security.Authorization;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Courses;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.GenericAttributes)]
    public partial class GenericAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public GenericAttributeController(IGenericAttributeService genericAttributeService, ICacheManager cacheManager, ILocalizationService localizationService,
            IWorkContext workContext, IPermissionService permissionService, IServiceProvider serviceProvider)
        {
            _genericAttributeService = genericAttributeService;
            _cacheManager = cacheManager;
            _localizationService = localizationService;
            _workContext = workContext;
            _permissionService = permissionService;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Utilities

        protected async Task<bool> CheckPermission(string objectType, string entityId)
        {
            Enum.TryParse(objectType, out EntityType objType);
            if (objType == EntityType.Category)
            {
                return await PermissionForCategory(entityId);
            }
            if (objType == EntityType.Product)
            {
                return await PermissionForProduct(entityId);
            }
            if (objType == EntityType.Manufacturer)
            {
                return await PermissionForManufacturer(entityId);
            }
            if (objType == EntityType.Course)
            {
                return await PermissionForCourse(entityId);
            }
            if (objType == EntityType.Order)
            {
                return await PermissionForOrder(entityId);
            }
            if (objType == EntityType.Customer)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                    return false;
            }
            if (objType == EntityType.CustomerRole)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageCustomerRoles))
                    return false;
            }
            if (objType == EntityType.Vendor)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                    return false;
            }
            if (objType == EntityType.Shipment)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageShipments))
                    return false;
            }
            if (objType == EntityType.ReturnRequest)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                    return false;
            }
            if (objType == EntityType.Topic)
            {
                if (!await _permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                    return false;
            }
            if (objType == EntityType.BlogPost)
            {
                return await PermissionForBlog(entityId);
            }
            return true;
        }
        protected async Task<bool> PermissionForCategory(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return false;

            var categoryService = _serviceProvider.GetRequiredService<ICategoryService>();
            var category = await categoryService.GetCategoryById(id);
            if (category != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!category.LimitedToStores || (category.LimitedToStores && category.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && category.Stores.Count > 1))
                        return false;
                    else
                    {
                        if (!category.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
        protected async Task<bool> PermissionForProduct(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return false;

            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var product = await productService.GetProductById(id);
            if (product != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!product.LimitedToStores || (product.LimitedToStores && product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.Stores.Count > 1))
                        return false;
                    else
                    {
                        if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            return false;
                    }
                }
                if (_workContext.CurrentVendor != null)
                {
                    if (product.VendorId != _workContext.CurrentVendor.Id)
                        return false;
                }
                return true;
            }
            return false;
        }
        protected async Task<bool> PermissionForManufacturer(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return false;

            var manufacturerService = _serviceProvider.GetRequiredService<IManufacturerService>();
            var manufacturer = await manufacturerService.GetManufacturerById(id);
            if (manufacturer != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!manufacturer.LimitedToStores || (manufacturer.LimitedToStores && manufacturer.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && manufacturer.Stores.Count > 1))
                        return false;
                    else
                    {
                        if (!manufacturer.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
        protected async Task<bool> PermissionForCourse(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageCourses))
                return false;

            var courseService = _serviceProvider.GetRequiredService<ICourseService>();
            var course = await courseService.GetById(id);
            if (course != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!course.LimitedToStores || (course.LimitedToStores && course.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && course.Stores.Count > 1))
                        return false;
                    else
                    {
                        if (!course.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
        protected async Task<bool> PermissionForOrder(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return false;

            var orderService = _serviceProvider.GetRequiredService<IOrderService>();
            var order = await orderService.GetOrderById(id);
            if (order != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
                        return false;
                }
                if (_workContext.CurrentVendor != null)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        protected async Task<bool> PermissionForBlog(string id)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return false;

            var blogService = _serviceProvider.GetRequiredService<IBlogService>();
            var blog = await blogService.GetBlogPostById(id);
            if (blog != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!blog.LimitedToStores || (blog.LimitedToStores && blog.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && blog.Stores.Count > 1))
                        return false;
                    else
                    {
                        if (!blog.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion


        #region Methods

        [HttpPost]
        public async Task<IActionResult> Add(GenericAttributeModel model)
        {
            if (!await CheckPermission(model.ObjectType, model.Id))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Common.GenericAttributes.Permission"));
            }

            if (ModelState.IsValid)
            {
                if (model.SelectedTab > 0)
                    TempData["Grand.selected-tab-index"] = model.SelectedTab;

                await _genericAttributeService.SaveAttribute(model.ObjectType, model.Id, model.Key, model.Value, model.StoreId);

                //TO DO - temporary solution
                //After add new attribute we need clear cache
                await _cacheManager.Clear();

                return Json(new
                {
                    success = true,
                });
            }
            return Json(new
            {
                success = false,
                errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                               .Select(m => m.ErrorMessage).ToArray()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(GenericAttributeModel model)
        {
            if (!await CheckPermission(model.ObjectType, model.Id))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Common.GenericAttributes.Permission"));
            }

            if (ModelState.IsValid)
            {
                if (model.SelectedTab > 0)
                    TempData["Grand.selected-tab-index"] = model.SelectedTab;

                await _genericAttributeService.SaveAttribute(model.ObjectType, model.Id, model.Key, string.Empty, model.StoreId);
                //TO DO - temporary solution
                //After delete attribute we need clear cache
                await _cacheManager.Clear();
                return Json(new
                {
                    success = true,
                });
            }
            return Json(new
            {
                success = false,
                errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                               .Select(m => m.ErrorMessage).ToArray()
            });
        }

        #endregion

    }
}