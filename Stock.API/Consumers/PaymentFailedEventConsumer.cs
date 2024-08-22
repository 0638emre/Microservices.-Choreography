using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer(MongoDBServices mongoDbServices) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        IMongoCollection<Models.Stock> collection = mongoDbServices.GetCollection<Models.Stock>();

        //stock güncellenmesi
        foreach (var orderItem in context.Message.OrderItems)
        {
            Models.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId))
                .FirstOrDefaultAsync();
            if (stock != null)
            {
                stock.Count += orderItem.Count;
                //mongo db de update ediyoruz
                await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }
        }
    }
}