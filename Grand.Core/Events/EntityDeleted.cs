using Grand.Domain;
using MediatR;

namespace Grand.Core.Events
{
    /// <summary>
    /// A container for passing entities that have been deleted. This is not used for entities that are deleted logicaly via a bit column.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityDeleted<T> : INotification where T : ParentEntity

    {
        public EntityDeleted(T entity)
        {
            Entity = entity;
        }
        public T Entity { get; private set; }
    }
}
