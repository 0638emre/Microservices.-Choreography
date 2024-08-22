using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers;

public class StockNotReservedEventConsumer(OrderAPIDBContext orderApiDbContext) : IConsumer<StockNotReservedEvent>
{
    public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
    {
        var order = orderApiDbContext.Orders.SingleOrDefault(o => o.Id == context.Message.OrderId);
        if (order is null)
            throw new Exception($"Order with id {context.Message.OrderId} not found");

        order.OrderStatu = OrderStatu.Fail;
        await orderApiDbContext.SaveChangesAsync();    }
}