using MassTransit;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Models.Context;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StockDBContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("StockDB")));

builder.Services.AddMassTransit(configurator =>
{
    //burada consumer olarak hangi consumerı tanıyacağını veriyoruz.
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        //burada rabbit mq bağlantısı yapıyoruz.
        //burada şu şekilde de rabbit mq bağlantısı verilebilirdi
        //configure.Host(builder.Configuration["RabbitMqBağlantiDizesi"]);
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        //burada rabbit mq nın dinleyeceği(consume edeceği endpointi veriyoruz)
        configure.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEventQueue, e=>
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        
        configure.ReceiveEndpoint(RabbitMqSettings.Stock_PaymentFailedEventQueue, e=>
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});

builder.Services.AddTransient<MongoDBServices>(); 

var app = builder.Build();

// using IServiceScope serviceScope = app.Services.CreateScope();
// MongoDBServices mongoDBservice = serviceScope.ServiceProvider.GetService<MongoDBServices>();
// var stockCollection = mongoDBservice.GetCollection<Stock.API.Models.Stock>();
// if (!stockCollection.FindSync(session => true).Any())
// {
//     await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 2000 });
//     await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 1000 });
//     await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 2400 });
//     await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 3500 });
//     await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 800 });
// }

app.Run();