using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.OrderTags)]
    public class OrderTagsController : BaseAdminController
    {
        private readonly IOrderTagService _orderTagService;
        private readonly IOrderService _orderService;

        public OrderTagsController(IOrderTagService orderTagService, IOrderService orderService)
        {
            _orderTagService = orderTagService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var tags = (await _orderTagService.GetAllOrderTags());
            var orderTagsList = new List<OrderTagModel>();
            foreach (var tag in tags)
            {
                var item = new OrderTagModel();
                item.Id = tag.Id;
                item.Name = tag.Name;
                item.OrderCount = await _orderTagService.GetOrderCount(tag.Id, "");
                orderTagsList.Add(item);
            }

            var gridModel = new DataSourceResult {
                Data = orderTagsList.OrderByDescending(x => x.OrderCount).PagedForCommand(command),
                Total = tags.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> Orders(string tagId, DataSourceRequest command)
        {
            var tag = await _orderTagService.GetOrderTagById(tagId);

            var orders = (await _orderService.SearchOrders(pageIndex: command.Page - 1, pageSize: command.PageSize, orderTagId: tag.Id)).ToList();
            var gridModel = new DataSourceResult {
                Data = orders.Select(x => new
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber
                }),
                Total = orders.Count
            };

            return Json(gridModel);
        }

        //edit
        public async Task<IActionResult> Edit(string id)
        {
            var orderTag = await _orderTagService.GetOrderTagById(id);
            if (orderTag == null)
                return RedirectToAction("List");

            var model = new OrderTagModel {
                Id = orderTag.Id,
                Name = orderTag.Name,
                OrderCount = await _orderTagService.GetOrderCount(orderTag.Id, "")
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrderTagModel model)
        {
            var orderTag = await _orderTagService.GetOrderTagById(model.Id);
            if (orderTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                orderTag.Name = model.Name;
                await _orderTagService.UpdateOrderTag(orderTag);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var tagOrder = await _orderTagService.GetOrderTagById(id);
            if (tagOrder == null)
                throw new ArgumentException("No order's tag found with the specified id");
            if (ModelState.IsValid)
            {
                await _orderTagService.DeleteOrderTag(tagOrder);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
    }
}
