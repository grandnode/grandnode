using FluentValidation;
using Grand.Core.Infrastructure;
using Grand.Services.Events;

namespace Grand.Framework.Validators
{
    public abstract class BaseGrandValidator<T> : AbstractValidator<T> where T : class
    {
        protected BaseGrandValidator()
        {
            PostInitialize();
        }

        /// <summary>
        /// Developers can override this method in custom partial classes
        /// in order to add some custom initialization code to constructors
        /// </summary>
        protected virtual void PostInitialize()
        {
            EngineContext.Current.Resolve<IEventPublisher>().Publish(this);
        }


    }
}