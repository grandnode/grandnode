using MediatR;

namespace Grand.Core.Events
{
    public class EntityCacheEvent : INotification
    {
        public EntityCacheEvent(string entity, CacheEvent cacheevent)
        {
            Entity = entity;
            Event = cacheevent;
        }
        public string Entity { get; private set; }
        public CacheEvent Event { get; private set; }
    }
}
