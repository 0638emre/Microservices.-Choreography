using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Enums;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMqSettings.Order_PaymentCompletedEventQueue, e=> 
            e.Consumer<PaymentCompletedEventConsumer>(context));
        
        configure.ReceiveEndpoint(RabbitMqSettings.Order_PaymentFailedEventQueue, e=> 
            e.Consumer<PaymentFailedEventConsumer>(context));
        
        configure.ReceiveEndpoint(RabbitMqSettings.Order_StockNotReseervedEventQueue, e=> 
            e.Consumer<StockNotReservedEventConsumer>(context));
    });
});

builder.Services.AddDbContext<OrderAPIDBContext>(ops =>
    ops.UseSqlServer(builder.Configuration.GetConnectionString("OrderAPIDB")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/create-order", async (CreateOrderVM model, OrderAPIDBContext context, IPublishEndpoint publishEndpoint) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        OrderItems = model.OrderItems.Select(oi => new OrderItem
        {
            ProductId = oi.ProductId,
            Count = oi.Count,
            Price = oi.Price
        }).ToList(),
        OrderStatu = OrderStatu.Suspend,
        TotalPrice = model.OrderItems.Sum(oi => oi.Price * oi.Count),
        CreatedDate = DateTime.UtcNow,
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    //buraya kadar order db de ilgili order ı oluşturduk şimdi sırada bu orderı stock api ye haber vermek için ilgili eventi publish edeceğiz
    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage
        {
            ProductId = oi.ProductId,
            Count = oi.Count,
            Price = oi.Price,
        }).ToList()
    };
    
    await publishEndpoint.Publish(orderCreatedEvent);
});

app.Run();