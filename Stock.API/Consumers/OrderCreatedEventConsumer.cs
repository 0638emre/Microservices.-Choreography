using MassTransit;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Models.Context;
using Stock.API.Services;

namespace Stock.API.Consumers;

//.net 8 ile gelen dependency injecktion
public class OrderCreatedEventConsumer(
    // MongoDBServices mongoDbServices,
    StockDBContext stockDBContext,
    ISendEndpointProvider sendEndpointProvider,
    IPublishEndpoint publishEndpoint)
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var productIds = context.Message.OrderItems.Select(o => o.ProductId).ToList();

        var stocks = await stockDBContext.Stocks
            .Where(s => productIds.Contains(s.ProductId))
            .ToListAsync();

        var stockResult = context.Message.OrderItems.All(orderItem =>
            stocks.Any(stock => stock.ProductId == orderItem.ProductId && stock.Count >= orderItem.Count)
        );


        if (stockResult)
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == orderItem.ProductId);
                if (stock is not null)
                {
                    stock.Count -= orderItem.Count;
                }
            }

            await stockDBContext.SaveChangesAsync();


            StockReservedEvent stockReservedEvent = new()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                TotalPrice = context.Message.TotalPrice,
                OrderItems = context.Message.OrderItems
            };
            
            //paymenti uyaracak event in fırlatılması. stocklar ok ise ödeme microservisine geçecek.
            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Payment_StockReservedEventQueue}"));
            await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            //stock işlemi başarısız
            //order ı uyaracak event in fırlatılması
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                Message = "Stok miktarı yetersiz."
            };
            
            await publishEndpoint.Publish(stockNotReservedEvent);
        }
    }
}