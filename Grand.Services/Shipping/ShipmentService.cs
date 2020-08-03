using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Shipping;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    /// <summary>
    /// Shipment service
    /// </summary>
    public partial class ShipmentService : IShipmentService
    {
        #region Fields

        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<ShipmentNote> _shipmentNoteRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shipmentRepository">Shipment repository</param>
        /// <param name="shipmentNoteRepository">Order note repository</param>
        /// <param name="mediator">Mediator</param>
        public ShipmentService(IRepository<Shipment> shipmentRepository,IRepository<ShipmentNote> shipmentNoteRepository,
            IMediator mediator)
        {
            _shipmentRepository = shipmentRepository;
            _shipmentNoteRepository = shipmentNoteRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual async Task DeleteShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            await _shipmentRepository.DeleteAsync(shipment);

            //event notification
            await _mediator.EntityDeleted(shipment);
        }

        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier, only shipments with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="shippingCountryId">Shipping country identifier; "" to load all records</param>
        /// <param name="shippingStateId">Shipping state identifier; "" to load all records</param>
        /// <param name="shippingCity">Shipping city; null to load all records</param>
        /// <param name="trackingNumber">Search by tracking number</param>
        /// <param name="loadNotShipped">A value indicating whether we should load only not shipped shipments</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Shipments</returns>
        public virtual async Task<IPagedList<Shipment>> GetAllShipments(string storeId = "", string vendorId = "", string warehouseId = "",
            string shippingCountryId = "",
            int shippingStateId = 0,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var builder = Builders<Shipment>.Filter;
            var filter = builder.Where(s => s.OrderId != "");

            if (!string.IsNullOrEmpty(storeId))
            {
                filter = filter & builder.Where(x => x.StoreId == storeId);
            }
            if (!string.IsNullOrEmpty(vendorId))
            {
                filter = filter & builder.Where(x => x.VendorId == vendorId);
            }
            if (!string.IsNullOrEmpty(trackingNumber))
                filter = filter & builder.Where(s => s.TrackingNumber.Contains(trackingNumber));

            if (loadNotShipped)
                filter = filter & builder.Where(s => !s.ShippedDateUtc.HasValue);
            if (createdFromUtc.HasValue)
                filter = filter & builder.Where(s => createdFromUtc.Value <= s.CreatedOnUtc);
            if (createdToUtc.HasValue)
                filter = filter & builder.Where(s => createdToUtc.Value >= s.CreatedOnUtc);

            var builderSort = Builders<Shipment>.Sort.Descending(x => x.CreatedOnUtc);

            var query = _shipmentRepository.Collection;
            var shipments = await PagedList<Shipment>.Create(query, filter, builderSort, pageIndex, pageSize);
            return shipments;
            
        }

        /// <summary>
        /// Get shipment by identifiers
        /// </summary>
        /// <param name="shipmentIds">Shipment identifiers</param>
        /// <returns>Shipments</returns>
        public virtual async Task<IList<Shipment>> GetShipmentsByIds(string[] shipmentIds)
        {
            if (shipmentIds == null || shipmentIds.Length == 0)
                return new List<Shipment>();

            var query = from o in _shipmentRepository.Table
                        where shipmentIds.Contains(o.Id)
                        select o;
            var shipments = await query.ToListAsync();

            //sort by passed identifiers
            var sortedOrders = new List<Shipment>();
            foreach (string id in shipmentIds)
            {
                var shipment = shipments.Find(x => x.Id == id);
                if (shipment != null)
                    sortedOrders.Add(shipment);
            }
            return sortedOrders;
        }


        public virtual async Task<IList<Shipment>> GetShipmentsByOrder(string orderId)
        {
            return await _shipmentRepository.Collection.Find(x => x.OrderId == orderId).ToListAsync();
        }

        /// <summary>
        /// Gets a shipment
        /// </summary>
        /// <param name="shipmentId">Shipment identifier</param>
        /// <returns>Shipment</returns>
        public virtual Task<Shipment> GetShipmentById(string shipmentId)
        {
            return _shipmentRepository.GetByIdAsync(shipmentId);
        }

        /// <summary>
        /// Inserts a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual async Task InsertShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");
            var shipmentExists = _shipmentRepository.Table.FirstOrDefault();
            shipment.ShipmentNumber = shipmentExists != null ? _shipmentRepository.Table.Max(x=>x.ShipmentNumber) + 1 : 1;
            await _shipmentRepository.InsertAsync(shipment);

            //event notification
            await _mediator.EntityInserted(shipment);
        }

        /// <summary>
        /// Updates the shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        public virtual async Task UpdateShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            await _shipmentRepository.UpdateAsync(shipment);

            //event notification
            await _mediator.EntityUpdated(shipment);
        }

        /// <summary>
        /// Get quantity in shipments. For example, get planned quantity to be shipped
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="ignoreShipped">Ignore already shipped shipments</param>
        /// <param name="ignoreDelivered">Ignore already delivered shipments</param>
        /// <returns>Quantity</returns>
        public virtual async Task<int> GetQuantityInShipments(Product product, string attributexml, string warehouseId,
            bool ignoreShipped, bool ignoreDelivered)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
                return 0;
            if (!product.UseMultipleWarehouses)
                return 0;

            var query = _shipmentRepository.Table;
            if (!String.IsNullOrEmpty(warehouseId))
                query = query.Where(si => si.ShipmentItems.Any(x=>x.WarehouseId == warehouseId));
            if (ignoreShipped)
                query = query.Where(si => !si.ShippedDateUtc.HasValue);
            if (ignoreDelivered)
                query = query.Where(si => !si.DeliveryDateUtc.HasValue);

            query = query.Where(si => si.ShipmentItems.Any(x => x.ProductId == product.Id));
            if(!string.IsNullOrEmpty(attributexml))
                query = query.Where(si => si.ShipmentItems.Any(x => x.AttributeXML == attributexml));

            var result = await query.SelectMany(x => x.ShipmentItems).Where(x => x.ProductId == product.Id).SumAsync(x => x.Quantity);

            return result;
        }

        #region Shipment notes

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="shipmentNote">The order note</param>
        public virtual async Task DeleteShipmentNote(ShipmentNote shipmentNote)
        {
            if (shipmentNote == null)
                throw new ArgumentNullException("shipmentNote");

            await _shipmentNoteRepository.DeleteAsync(shipmentNote);

            //event notification
            await _mediator.EntityDeleted(shipmentNote);
        }

        /// <summary>
        /// Deletes an shipment note
        /// </summary>
        /// <param name="shipmentNote">The shipment note</param>
        public virtual async Task InsertShipmentNote(ShipmentNote shipmentNote)
        {
            if (shipmentNote == null)
                throw new ArgumentNullException("shipmentNote");

            await _shipmentNoteRepository.InsertAsync(shipmentNote);

            //event notification
            await _mediator.EntityInserted(shipmentNote);
        }

        public virtual async Task<IList<ShipmentNote>> GetShipmentNotes(string shipmentId)
        {
            var query = from shipmentNote in _shipmentNoteRepository.Table
                        where shipmentNote.ShipmentId == shipmentId
                        orderby shipmentNote.CreatedOnUtc descending
                        select shipmentNote;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get shipmentnote by id
        /// </summary>
        /// <param name="shipmentnoteId">Shipment note identifier</param>
        /// <returns>shipmentNote</returns>
        public virtual Task<ShipmentNote> GetShipmentNote(string shipmentnoteId)
        {
            return _shipmentNoteRepository.Table.Where(x => x.Id == shipmentnoteId).FirstOrDefaultAsync();
        }


        #endregion
        #endregion
    }
}
