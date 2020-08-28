namespace Grand.Framework.Validators
{
    public interface IValidatorConsumer<T> where T : class
    {
        void AddRules(BaseGrandValidator<T> validator);
    }
}
