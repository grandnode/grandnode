#! "netcoreapp2.2"
#r "Grand.Core"
#r "Grand.Services"

using System;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Services.Events;

/* Sample code to add new token message (message email) to the order */

public class OrderTokenTest : IConsumer<EntityTokensAddedEvent<Order>>
{

    public void HandleEvent(EntityTokensAddedEvent<Order> eventMessage)
    {
        eventMessage.LiquidObject.AdditionalTokens.Add("NewOrderNumber", $"{eventMessage.Entity.CreatedOnUtc.Year}/{eventMessage.Entity.OrderNumber}");
        //in message templates you can put new token {{AdditionalTokens["NewOrderNumber"]}}
    }
}


