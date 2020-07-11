using DotLiquid;
using Grand.Domain;
using Grand.Domain.Messages;
using MediatR;

namespace Grand.Services.Notifications.Messages
{
    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="liquidDrop">DotLiquid Drop, e.g. LiquidOrder</param>
    /// <param name="liquidObject">An object that acumulates all DotLiquid Drops</param>
    public class EntityTokensAddedEvent<T> : INotification where T : ParentEntity
    {
        private readonly T _entity;
        private readonly Drop _liquidDrop;
        private readonly LiquidObject _liquidObject;

        public EntityTokensAddedEvent(T entity, Drop liquidDrop, LiquidObject liquidObject)
        {
            _entity = entity;
            _liquidDrop = liquidDrop;
            _liquidObject = liquidObject;
        }

        public T Entity { get { return _entity; } }
        public Drop LiquidDrop { get { return _liquidDrop; } }
        public LiquidObject LiquidObject { get { return _liquidObject; } }
    }
}
