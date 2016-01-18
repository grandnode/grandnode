using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Shipment service
    /// </summary>
    public partial class ShipmentService : IShipmentService
    {
        #region Fields

        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IEventPublisher _eventPublisher;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shipmentRepository">Shipment repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ShipmentService(IRepository<Shipment> shipmentRepository,
            IEventPublisher eventPublisher)
        {
            this._shipmentRepository = shipmentRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void DeleteShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            _shipmentRepository.Delete(shipment);

            //event notification
            _eventPublisher.EntityDeleted(shipment);
        }
        
        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier, only shipments with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="shippingCountryId">Shipping country identifier; 0 to load all records</param>
        /// <param name="shippingStateId">Shipping state identifier; 0 to load all records</param>
        /// <param name="shippingCity">Shipping city; null to load all records</param>
        /// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="loadNotShipped">A value indicating whether we should load only not shipped shipments</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Shipments</returns>
        public virtual IPagedList<Shipment> GetAllShipments(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var builder = Builders<Shipment>.Filter;
            var filter = builder.Where(s => s.OrderId != 0);

            //var query = _shipmentRepository.Table;
            if (!String.IsNullOrEmpty(trackingNumber))
                filter = filter & builder.Where(s => s.TrackingNumber.Contains(trackingNumber));
            /*
            if (shippingCountryId > 0)
                query = query.Where(s => s.Order.ShippingAddress.CountryId == shippingCountryId);
            if (shippingStateId > 0)
                query = query.Where(s => s.Order.ShippingAddress.StateProvinceId == shippingStateId);
            if (!String.IsNullOrWhiteSpace(shippingCity))
                query = query.Where(s => s.Order.ShippingAddress.City.Contains(shippingCity));
            */
            if (loadNotShipped)
                filter = filter & builder.Where(s => !s.ShippedDateUtc.HasValue);
            if (createdFromUtc.HasValue)
                filter = filter & builder.Where(s => createdFromUtc.Value <= s.CreatedOnUtc);
            if (createdToUtc.HasValue)
                filter = filter & builder.Where(s => createdToUtc.Value >= s.CreatedOnUtc);
            /*
            if (vendorId > 0)
            {
                var queryVendorOrderItems = from orderItem in _orderItemRepository.Table
                                             where orderItem.Product.VendorId == vendorId
                                             select orderItem.Id;

                query = from s in query
                        where queryVendorOrderItems.Intersect(s.ShipmentItems.Select(si => si.OrderItemId)).Any()
                        select s;
            }
            
            if (warehouseId > 0)
            {
                query = from s in query
                        where s.ShipmentItems.Any(si => si.WarehouseId == warehouseId)
                        select s;
            }
            */
            //query = query.OrderByDescending(s => s.CreatedOnUtc);

            var builderSort = Builders<Shipment>.Sort.Descending(x => x.CreatedOnUtc);

            var query = _shipmentRepository.Collection;
            var shipments = new PagedList<Shipment>(query, filter, builderSort, pageIndex, pageSize);

            return shipments;
            
        }

        /// <summary>
        /// Get shipment by identifiers
        /// </summary>
        /// <param name="shipmentIds">Shipment identifiers</param>
        /// <returns>Shipments</returns>
        public virtual IList<Shipment> GetShipmentsByIds(int[] shipmentIds)
        {
            if (shipmentIds == null || shipmentIds.Length == 0)
                return new List<Shipment>();

            var query = from o in _shipmentRepository.Table
                        where shipmentIds.Contains(o.Id)
                        select o;
            var shipments = query.ToList();
            //sort by passed identifiers
            var sortedOrders = new List<Shipment>();
            foreach (int id in shipmentIds)
            {
                var shipment = shipments.Find(x => x.Id == id);
                if (shipment != null)
                    sortedOrders.Add(shipment);
            }
            return sortedOrders;
        }


        public virtual IList<Shipment> GetShipmentsByOrder(int orderId)
        {
            return _shipmentRepository.Collection.Find(x => x.OrderId == orderId).ToListAsync().Result;
        }

        /// <summary>
        /// Gets a shipment
        /// </summary>
        /// <param name="shipmentId">Shipment identifier</param>
        /// <returns>Shipment</returns>
        public virtual Shipment GetShipmentById(int shipmentId)
        {
            if (shipmentId == 0)
                return null;

            return _shipmentRepository.GetById(shipmentId);
        }

        /// <summary>
        /// Inserts a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void InsertShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            _shipmentRepository.Insert(shipment);

            //event notification
            _eventPublisher.EntityInserted(shipment);
        }

        /// <summary>
        /// Updates the shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual void UpdateShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            _shipmentRepository.Update(shipment);

            //event notification
            _eventPublisher.EntityUpdated(shipment);
        }

        /// <summary>
        /// Get quantity in shipments. For example, get planned quantity to be shipped
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="ignoreShipped">Ignore already shipped shipments</param>
        /// <param name="ignoreDelivered">Ignore already delivered shipments</param>
        /// <returns>Quantity</returns>
        public virtual int GetQuantityInShipments(Product product, int warehouseId,
            bool ignoreShipped, bool ignoreDelivered)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                return 0;
            if (!product.UseMultipleWarehouses)
                return 0;

            var query = _shipmentRepository.Table;
            if (warehouseId > 0)
                query = query.Where(si => si.ShipmentItems.Any(x=>x.WarehouseId == warehouseId));
            if (ignoreShipped)
                query = query.Where(si => !si.ShippedDateUtc.HasValue);
            if (ignoreDelivered)
                query = query.Where(si => !si.DeliveryDateUtc.HasValue);

            query = query.Where(si => si.ShipmentItems.Any(x => x.ProductId == product.Id));
            var result = query.SelectMany(x => x.ShipmentItems).Where(x => x.ProductId == product.Id).Sum(x => x.Quantity);

            return result;
        }


        #endregion
    }
}
