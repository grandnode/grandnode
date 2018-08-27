#! "netcoreapp2.1"
#r "Grand.Core"
#r "Grand.Services"

using System;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Services.Events;
using Grand.Services.Messages;

/* Sample code to add new token message (message email) to the order */

public class EntityTokensAddedEventConsumer : IConsumer<EntityTokensAddedEvent<Order, Token>>
{
    public void HandleEvent(EntityTokensAddedEvent<Order, Token> eventMessage)
    {
        eventMessage.Tokens.Add(new Token("order", "my value"));
    }
}

