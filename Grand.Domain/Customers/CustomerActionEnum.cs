namespace Grand.Domain.Customers
{
    public enum CustomerActionTypeEnum
    {
        AddToCart = 1,
        AddOrder = 2,
        Viewed = 3,
        Url = 4,
        Registration = 5,
        PaidOrder = 6
    }
    public enum CustomerActionConditionEnum
    {
        OneOfThem = 0,
        AllOfThem = 1,
    }

    public enum CustomerActionConditionTypeEnum
    {
        Product = 1,
        Category = 2,
        Manufacturer = 3,
        Vendor = 4,
        ProductAttribute = 5,
        ProductSpecification = 6,
        CustomerRole = 7,
        CustomerTag = 8,
        CustomerRegisterField = 9,
        CustomCustomerAttribute = 10,
        UrlReferrer = 11,
        UrlCurrent = 12,
        Store = 13
    }

    public enum CustomerReactionTypeEnum
    {
        Banner = 1,
        Email = 2,
        AssignToCustomerRole = 3,
        AssignToCustomerTag = 4,
        InteractiveForm = 5,
    }

    
}
