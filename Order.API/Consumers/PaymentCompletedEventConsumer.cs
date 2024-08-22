using MassTransit;
using Order.API.Enums;
using Order.API.Models;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer(OrderAPIDBContext orderApiDbContext) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = orderApiDbContext.Orders.SingleOrDefault(o => o.Id == context.Message.OrderId);
        if (order is null)
            throw new Exception($"Order with id {context.Message.OrderId} not found");

        order.OrderStatu = OrderStatu.Completed;
        await orderApiDbContext.SaveChangesAsync();
    }
}