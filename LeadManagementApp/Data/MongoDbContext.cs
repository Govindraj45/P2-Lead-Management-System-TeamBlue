using LeadManagementSystem.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LeadManagementSystem.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public IMongoCollection<Lead> Leads { get; }
    public IMongoCollection<SalesRep> SalesRepresentatives { get; }
    public IMongoCollection<Interaction> Interactions { get; }
    public IMongoCollection<BsonDocument> Counters { get; }

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);

        Leads = _database.GetCollection<Lead>(settings.LeadsCollectionName);
        SalesRepresentatives = _database.GetCollection<SalesRep>(settings.SalesRepsCollectionName);
        Interactions = _database.GetCollection<Interaction>(settings.InteractionsCollectionName);
        Counters = _database.GetCollection<BsonDocument>(settings.CountersCollectionName);

        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        Leads.Indexes.CreateOne(
            new CreateIndexModel<Lead>(
                Builders<Lead>.IndexKeys.Ascending(x => x.LeadId),
                new CreateIndexOptions { Unique = true }));

        SalesRepresentatives.Indexes.CreateOne(
            new CreateIndexModel<SalesRep>(
                Builders<SalesRep>.IndexKeys.Ascending(x => x.RepId),
                new CreateIndexOptions { Unique = true }));

        Interactions.Indexes.CreateOne(
            new CreateIndexModel<Interaction>(
                Builders<Interaction>.IndexKeys.Ascending(x => x.InteractionId),
                new CreateIndexOptions { Unique = true }));

        Interactions.Indexes.CreateOne(
            new CreateIndexModel<Interaction>(
                Builders<Interaction>.IndexKeys.Ascending(x => x.LeadId)));
    }
}
