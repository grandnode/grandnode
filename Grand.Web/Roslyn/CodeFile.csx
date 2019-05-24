#! "netcoreapp2.2"
#r "Grand.Core"
#r "Grand.Services"

using System;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Services.Events;
using System.Threading.Tasks;
/* Sample code to add new token message (message email) to the order */

public class OrderTokenTest : IConsumer<EntityTokensAddedEvent<Order>>
{

    public Task HandleEventAsync(EntityTokensAddedEvent<Order> eventMessage)
    {
        //in message templates you can put new token {{AdditionalTokens["NewOrderNumber"]}}
        eventMessage.LiquidObject.AdditionalTokens.Add("NewOrderNumber", $"{eventMessage.Entity.CreatedOnUtc.Year}/{eventMessage.Entity.OrderNumber}");
        return Task.CompletedTask;
    }
    public void HandleEvent(EntityTokensAddedEvent<Order> eventMessage) { }
}


