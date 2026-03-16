using MongoDB.Bson;
using MongoDB.Driver;

namespace LeadManagementSystem.Data;

public class MongoSequenceService
{
    private readonly MongoDbContext _context;

    public MongoSequenceService(MongoDbContext context)
    {
        _context = context;
    }

    public int GetNextValue(string sequenceName)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sequenceName);
        var update = Builders<BsonDocument>.Update.Inc("value", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var updatedDoc = _context.Counters.FindOneAndUpdate(filter, update, options);
        return (int)updatedDoc["value"].ToInt64();
    }
}
