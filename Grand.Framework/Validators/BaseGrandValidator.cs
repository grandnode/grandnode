using FluentValidation;
using Grand.Core.Infrastructure;

namespace Grand.Framework.Validators
{
    public abstract class BaseGrandValidator<T> : AbstractValidator<T> where T : class
    {
        protected BaseGrandValidator()
        {
            PostInitialize();
        }

        protected virtual void PostInitialize()
        {
            var validator = EngineContext.Current.ResolveAll<IValidatorConsumer<T>>();
            foreach (var item in validator)
            {
                item.AddRules(this);
            }

        }

    }


}