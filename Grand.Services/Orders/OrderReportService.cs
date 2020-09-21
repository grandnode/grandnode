using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Services.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order report service
    /// </summary>
    public partial class OrderReportService : IOrderReportService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ProductAlsoPurchased> _productAlsoPurchasedRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="productAlsoPurchasedRepository">Product also purchased repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="dateTimeHelper">Datetime helper</param>
        public OrderReportService(IRepository<Order> orderRepository,
            IRepository<ProductAlsoPurchased> productAlsoPurchasedRepository,
            IRepository<Product> productRepository,
            IDateTimeHelper dateTimeHelper)
        {
            _orderRepository = orderRepository;
            _productAlsoPurchasedRepository = productAlsoPurchasedRepository;
            _productRepository = productRepository;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual async Task<IList<OrderByCountryReportLine>> GetCountryReport(string storeId, OrderStatus? os,
            PaymentStatus? ps, ShippingStatus? ss, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table;

            query = query.Where(o => !o.Deleted);
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(o => o.StoreId == storeId);
            if (orderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            if (shippingStatusId.HasValue)
                query = query.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            
            var report = (from oq in query
                        group oq by oq.BillingAddress.CountryId into result
                        select new
                        {
                            CountryId = result.Key,
                            TotalOrders = result.Count(),
                            SumOrders = result.Sum(o => o.OrderTotal)
                        }
                       )
                       .OrderByDescending(x => x.SumOrders)
                       .Select(r => new OrderByCountryReportLine
                       {
                           CountryId = r.CountryId,
                           TotalOrders = r.TotalOrders,
                           SumOrders = r.SumOrders
                       });

            return await report.ToListAsync();
        }


        /// <summary>
        /// Get "order by time" report
        /// </summary>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual async Task<IList<OrderByTimeReportLine>> GetOrderByTimeReport(string storeId, DateTime? startTimeUtc = null,
            DateTime? endTimeUtc = null)
        {
            List<OrderByTimeReportLine> report = new List<OrderByTimeReportLine>();
            if (!startTimeUtc.HasValue)
                startTimeUtc = DateTime.MinValue;
            if (!endTimeUtc.HasValue)
                endTimeUtc = DateTime.UtcNow;

            var endTime = new DateTime(endTimeUtc.Value.Year, endTimeUtc.Value.Month, endTimeUtc.Value.Day, 23, 59, 00);

            var builder = Builders<Order>.Filter;

            var filter = builder.Where(o => !o.Deleted);
            filter = filter & builder.Where(o => o.CreatedOnUtc >= startTimeUtc.Value && o.CreatedOnUtc <= endTime);

            if(!string.IsNullOrEmpty(storeId))
                filter = filter & builder.Where(o => o.StoreId == storeId);

            var daydiff = (endTimeUtc.Value - startTimeUtc.Value).TotalDays;
            if(daydiff > 31)
            {
                var query = await _orderRepository.Collection.Aggregate().Match(filter).Group(x =>
                    new { Year = x.CreatedOnUtc.Year, Month = x.CreatedOnUtc.Month },
                    g => new { Period = g.Key, Amount = g.Sum(x => x.OrderTotal), Count = g.Count() }).SortBy(x=>x.Period).ToListAsync();
                foreach (var item in query)
                {
                    report.Add(new OrderByTimeReportLine()
                    {
                        Time = item.Period.Year.ToString() + "-" + item.Period.Month.ToString(),
                        SumOrders = item.Amount,
                        TotalOrders = item.Count,
                    });
                }
            }
            else
            {
                var query = await _orderRepository.Collection.Aggregate().Match(filter).Group(x=>
                    new { Year = x.CreatedOnUtc.Year, Month = x.CreatedOnUtc.Month, Day = x.CreatedOnUtc.Day },
                    g => new { Period = g.Key, Amount = g.Sum(x => x.OrderTotal), Count = g.Count() }).SortBy(x => x.Period).ToListAsync();
                foreach (var item in query)
                {
                    report.Add(new OrderByTimeReportLine()
                    {
                        Time = item.Period.Year.ToString() + "-" + item.Period.Month.ToString()+"-" + item.Period.Day.ToString(),
                        SumOrders = item.Amount,
                        TotalOrders = item.Count,
                    });
                }
            }


            
            return report;
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="ignoreCancelledOrders">A value indicating whether to ignore cancelled orders</param>
        /// <param name="tagid">Tag ident.</param>
        /// <returns>Result</returns>
        public virtual async Task<OrderAverageReportLine> GetOrderAverageReportLine(string storeId = "",
            string vendorId = "", string billingCountryId = "", 
            string orderId = "", string paymentMethodSystemName = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingEmail = null, string billingLastName = "", bool ignoreCancelledOrders = false, 
            string tagid = null)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var builder = Builders<Order>.Filter;

            var filter = builder.Where(o => !o.Deleted);
            if (!String.IsNullOrEmpty(storeId))
                filter = filter & builder.Where(o => o.StoreId == storeId);

            if (!String.IsNullOrEmpty(orderId))
                filter = filter & builder.Where(o => o.StoreId == storeId);

            if (!String.IsNullOrEmpty(vendorId))
            {
                filter = filter & builder
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.VendorId == vendorId));
            }
            if (!String.IsNullOrEmpty(billingCountryId))
                filter = filter & builder.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);

            if (ignoreCancelledOrders)
            {
                var cancelledOrderStatusId = (int)OrderStatus.Cancelled;
                filter = filter & builder.Where(o => o.OrderStatusId != cancelledOrderStatusId);

            }
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                filter = filter & builder.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (orderStatusId.HasValue)
                filter = filter & builder.Where(o => o.OrderStatusId == orderStatusId.Value);

            if (paymentStatusId.HasValue)
                filter = filter & builder.Where(o => o.PaymentStatusId == paymentStatusId.Value);

            if (shippingStatusId.HasValue)
                filter = filter & builder.Where(o => o.ShippingStatusId == shippingStatusId.Value);

            if (startTimeUtc.HasValue)
                filter = filter & builder.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);

            if (endTimeUtc.HasValue)
                filter = filter & builder.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

            if (!String.IsNullOrEmpty(billingEmail))
                filter = filter & builder.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));

            if (!String.IsNullOrEmpty(billingLastName))
                filter = filter & builder.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));

            //tag filtering 
            if (!string.IsNullOrEmpty(tagid))
                filter = filter & builder.Where(o => o.OrderTags.Any(y => y == tagid));

            var query = await _orderRepository.Collection
                    .Aggregate()
                    .Match(filter)
                    .Group(x => 1, g => new OrderAverageReportLine
                    {
                        CountOrders = g.Count(),
                        SumShippingExclTax = g.Sum(o => o.OrderShippingExclTax),
                        SumTax = g.Sum(o => o.OrderTax),
                        SumOrders = g.Sum(o => o.OrderTotal)
                    }).ToListAsync();


            var item2 = query.Count() > 0 ? query.FirstOrDefault() : new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                SumTax = decimal.Zero,
                SumOrders = decimal.Zero,
            };
            return item2;
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <returns>Result</returns>
        public virtual async Task<OrderAverageReportLineSummary> OrderAverageReport(string storeId, OrderStatus os)
        {
            var item = new OrderAverageReportLineSummary();
            item.OrderStatus = os;

            DateTime nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            TimeZoneInfo timeZone = _dateTimeHelper.CurrentTimeZone;

            //today
            var t1 = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            if (!timeZone.IsInvalidTime(t1))
            {
                DateTime? startTime1 = _dateTimeHelper.ConvertToUtcTime(t1, timeZone);
                var todayResult = await GetOrderAverageReportLine(storeId: storeId,
                    os: os, 
                    startTimeUtc: startTime1);
                item.SumTodayOrders = todayResult.SumOrders;
                item.CountTodayOrders = todayResult.CountOrders;
            }
            //week
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var today = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            DateTime t2 = today.AddDays(-(today.DayOfWeek - fdow));
            if (!timeZone.IsInvalidTime(t2))
            {
                DateTime? startTime2 = _dateTimeHelper.ConvertToUtcTime(t2, timeZone);
                var weekResult = await GetOrderAverageReportLine(storeId: storeId,
                    os: os,
                    startTimeUtc: startTime2);
                item.SumThisWeekOrders = weekResult.SumOrders;
                item.CountThisWeekOrders = weekResult.CountOrders;
            }
            //month
            var t3 = new DateTime(nowDt.Year, nowDt.Month, 1);
            if (!timeZone.IsInvalidTime(t3))
            {
                DateTime? startTime3 = _dateTimeHelper.ConvertToUtcTime(t3, timeZone);
                var monthResult = await GetOrderAverageReportLine(storeId: storeId,
                    os: os,
                    startTimeUtc: startTime3);
                item.SumThisMonthOrders = monthResult.SumOrders;
                item.CountThisMonthOrders = monthResult.CountOrders;
            }
            //year
            var t4 = new DateTime(nowDt.Year, 1, 1);
            if (!timeZone.IsInvalidTime(t4))
            {
                DateTime? startTime4 = _dateTimeHelper.ConvertToUtcTime(t4, timeZone);
                var yearResult = await GetOrderAverageReportLine(storeId: storeId,
                    os: os,
                    startTimeUtc: startTime4);
                item.SumThisYearOrders = yearResult.SumOrders;
                item.CountThisYearOrders = yearResult.CountOrders;
            }
            //all time
            var allTimeResult = await GetOrderAverageReportLine(storeId: storeId, os: os);
            item.SumAllTimeOrders = allTimeResult.SumOrders;
            item.CountAllTimeOrders = allTimeResult.CountOrders;

            return item;
        }

        /// <summary>
        /// Get best sellers report
        /// </summary>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="categoryId">Category identifier; "" to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; "" to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; "" to load all records</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Result</returns>
        public virtual async Task<IPagedList<BestsellersReportLine>> BestSellersReport(
            string storeId = "", string vendorId = "",
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingCountryId = "",
            int orderBy = 1,
            int pageIndex = 0, int pageSize = int.MaxValue, 
            bool showHidden = false)
        {

            
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var builder = Builders<Order>.Filter;
            var builderItem = Builders<UnwindedOrderItem>.Filter;

            var filter = builder.Where(o => !o.Deleted);
            var filterItem = builderItem.Where(x => true);
            if (!String.IsNullOrEmpty(vendorId))
            {
                filterItem = filterItem & builderItem.Where(x => x.OrderItems.VendorId == vendorId);
            }

            if (!String.IsNullOrEmpty(storeId))
                filter = filter & builder.Where(o => o.StoreId == storeId);
            
            if (!String.IsNullOrEmpty(vendorId))
            {
                filter = filter & builder
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.VendorId == vendorId));
            }
            if (!String.IsNullOrEmpty(billingCountryId))
                filter = filter & builder.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
           
           
            if (orderStatusId.HasValue)
                filter = filter & builder.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                filter = filter & builder.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            if (shippingStatusId.HasValue)
                filter = filter & builder.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (createdFromUtc.HasValue)
                filter = filter & builder.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                filter = filter & builder.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            FilterDefinition<BsonDocument> filterPublishedProduct = new BsonDocument("Product.Published", true);
            var groupBy = new BsonDocument
            {
                 new BsonElement("_id", "$OrderItems.ProductId"),
                 new BsonElement("TotalAmount", new BsonDocument("$sum", "$OrderItems.PriceExclTax")),
                 new BsonElement("TotalQuantity", new BsonDocument("$sum", "$OrderItems.Quantity"))
            };

            var query = _orderRepository.Collection
                .Aggregate()
                .Match(filter)
                .Unwind<Order, UnwindedOrderItem>(x => x.OrderItems)
                .Match(filterItem)
                .Lookup("Product", "OrderItems.ProductId", "_id", "Product")
                .Match(filterPublishedProduct)
                .Group(groupBy);

            if (orderBy == 1)
            {
                query = query.SortByDescending(x=>x["TotalQuantity"]);
            }
            else
            {
                query = query.SortByDescending(x => x["TotalAmount"]);
            }

            var query2 = new List<BestsellersReportLine>();
            await query.ForEachAsync(q =>
            {
                var line = new BestsellersReportLine();
                line.ProductId = q["_id"].ToString();
                line.TotalAmount = q["TotalAmount"].AsDecimal;
                line.TotalQuantity = q["TotalQuantity"].AsInt32;
                query2.Add(line);
            });
            var result = new PagedList<BestsellersReportLine>(query2, pageIndex, pageSize);
            return result;
        }


        /// <summary>
        /// Gets a report of orders in the last days
        /// </summary>
        /// <param name="days">Orders in the last days</param>
        /// <param name="storeId">Store ident</param>
        /// <returns>ReportPeriodOrder</returns>
        public virtual async Task<ReportPeriodOrder> GetOrderPeriodReport(int days, string storeId)
        {
            DateTime date = days != 0 ? _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-days).Date : _dateTimeHelper.ConvertToUserTime(DateTime.Now).Date ;

            var query = from o in _orderRepository.Table
                        where !o.Deleted && o.CreatedOnUtc >= date
                        && (string.IsNullOrEmpty(storeId) || o.StoreId == storeId)
                        group o by 1 into g
                        select new ReportPeriodOrder() { Amount = g.Sum(x => x.OrderTotal), Count = g.Count() };
            var report = (await query.ToListAsync())?.FirstOrDefault();
            if (report == null)
                report = new ReportPeriodOrder();
            report.Date = date;
            return report;
        }



        /// <summary>
        /// Gets a list of products (identifiers) purchased by other customers who purchased a specified product
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual async Task<string[]> GetAlsoPurchasedProductsIds(string storeId, string productId,
            int recordsToReturn = 5, bool showHidden = false)
        {
            var product = from p in _productAlsoPurchasedRepository.Table
                          where p.ProductId == productId
                            group p by p.ProductId2 into g
                            select new
                            {
                                ProductId = g.Key,
                                ProductsPurchased = g.Sum(x => x.Quantity),
                            };
            product = product.OrderByDescending(x => x.ProductsPurchased);
            if (recordsToReturn > 0)
                product = product.Take(recordsToReturn);

            var report = await product.ToListAsync();
            var ids = new List<string>();
            foreach (var reportLine in report)
                ids.Add(reportLine.ProductId);

            return ids.ToArray();
        }

        /// <summary>
        /// Gets a list of products that were never sold
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual async Task<IPagedList<Product>> ProductsNeverSold(string storeId = "", string vendorId = "",
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {

            createdFromUtc = !createdFromUtc.HasValue ? DateTime.MinValue : createdFromUtc;
            createdToUtc = !createdToUtc.HasValue ? DateTime.MaxValue: createdToUtc;

            var query = (await (from order in _orderRepository.Table
                         where
                         (string.IsNullOrEmpty(storeId) || order.StoreId == storeId) &&
                         (createdFromUtc.Value <= order.CreatedOnUtc) &&
                         (createdToUtc.Value >= order.CreatedOnUtc) &&
                         (!order.Deleted)
                         from orderItem in order.OrderItems
                         select new { orderItem.ProductId }).ToListAsync()).Distinct().Select(x=>x.ProductId);

            var simpleProductTypeId = (int)ProductType.SimpleProduct;

            var qproducts = from p in _productRepository.Table
                         orderby p.Name
                         where (!query.Contains(p.Id)) &&
                               //include only simple products
                               (p.ProductTypeId == simpleProductTypeId) &&                               
                               (vendorId == "" || p.VendorId == vendorId) &&
                               (string.IsNullOrEmpty(storeId) || p.Stores.Contains(storeId) || p.LimitedToStores == false) &&
                               (showHidden || p.Published)
                         select p;

            return await PagedList<Product>.Create(qproducts, pageIndex, pageSize);
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="tagid">Tag ident.</param>
        /// <returns>Result</returns>
        public virtual async Task<decimal> ProfitReport(string storeId = "", string vendorId = "",
            string billingCountryId = "", string orderId = "", string paymentMethodSystemName = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingEmail = null, string billingLastName = "", string tagid = null)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table;

            query = query.Where(o => !o.Deleted);
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(o => o.StoreId == storeId);
            if (!String.IsNullOrEmpty(orderId))
                query = query.Where(o => o.Id == orderId);
            if (!String.IsNullOrEmpty(vendorId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.VendorId == vendorId));
            }
            if (!String.IsNullOrEmpty(billingCountryId))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (orderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            if (shippingStatusId.HasValue)
                query = query.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));

            //tag filtering 
            if (!string.IsNullOrEmpty(tagid))
                query = query.Where(o => o.OrderTags.Any(y => y == tagid));

            var query2 = from o in query
                    from p in o.OrderItems
                    select p;


            var productCost = await query2.SumAsync(orderItem => orderItem.OriginalProductCost * orderItem.Quantity);

            var reportSummary = await GetOrderAverageReportLine(
                storeId: storeId,
                vendorId: vendorId,
                billingCountryId: billingCountryId,
                orderId: orderId,
                paymentMethodSystemName: paymentMethodSystemName,
                os: os, 
                ps: ps, 
                ss: ss,
                startTimeUtc: startTimeUtc,
                endTimeUtc: endTimeUtc,
                billingEmail: billingEmail,
                tagid: tagid
                );
            var profit = Convert.ToDecimal(reportSummary.SumOrders - reportSummary.SumShippingExclTax - reportSummary.SumTax - productCost);
            return profit;
        }

        public class UnwindedOrderItem
        {
            public OrderItem OrderItems { get; set; }
        }


        #endregion
    }
}
