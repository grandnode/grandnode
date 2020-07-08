#! "netcoreapp3.1"
#r "Grand.Core"
#r "Grand.Services"

using System;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Services.Events;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

/* Sample code to add new token message (message email) to the order */

public class OrderTokenTest : INotificationHandler<EntityTokensAddedEvent<Order>>
{
    public Task Handle(EntityTokensAddedEvent<Order> eventMessage, CancellationToken cancellationToken)
    {
        //in message templates you can put new token {{AdditionalTokens["NewOrderNumber"]}}
        eventMessage.LiquidObject.AdditionalTokens.Add("NewOrderNumber", $"{eventMessage.Entity.CreatedOnUtc.Year}/{eventMessage.Entity.OrderNumber}");
        return Task.CompletedTask;
    }

}



