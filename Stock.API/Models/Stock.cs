using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Models;

// public class Stock
// {   [BsonId]
//     [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
//     [BsonElement(Order = 0)]
//     public Guid Id { get; set; }
//     
//     [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
//     [BsonElement(Order = 1)]
//     public Guid ProductId { get; set; }
//
//     [BsonRepresentation(BsonType.Int32)]
//     [BsonElement(Order = 2)]
//     public int Count { get; set; }
//
//     [BsonRepresentation(BsonType.DateTime)]
//     [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
//     [BsonElement(Order = 3)]
//     public DateTime CreatedTime { get; set; }
// }

public class Stock
{   
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Count { get; set; }
    public DateTime CreatedTime { get; set; }
}