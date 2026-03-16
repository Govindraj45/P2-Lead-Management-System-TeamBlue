namespace LeadManagementSystem.Data;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "LeadManagementSystem";
    public string LeadsCollectionName { get; set; } = "Leads";
    public string SalesRepsCollectionName { get; set; } = "SalesRepresentatives";
    public string InteractionsCollectionName { get; set; } = "Interactions";
    public string CountersCollectionName { get; set; } = "Counters";
}
