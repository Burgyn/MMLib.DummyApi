using System.Collections.Concurrent;
using System.Text.Json;
using LiteDB;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

public class CustomDataStore : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ConcurrentDictionary<string, JsonElement> _schemas = new();
    private readonly ConcurrentDictionary<string, BackgroundJobConfig> _backgroundConfigs = new();

    public CustomDataStore()
    {
        _db = new LiteDatabase("Filename=:memory:");
    }

    // Collection operations
    public IEnumerable<string> GetCollectionNames() 
        => _db.GetCollectionNames().Where(n => !n.StartsWith("_"));

    public bool CollectionExists(string collection) 
        => _db.CollectionExists(collection);

    public bool DeleteCollection(string collection)
    {
        _schemas.TryRemove(collection, out _);
        _backgroundConfigs.TryRemove(collection, out _);
        return _db.DropCollection(collection);
    }

    // Entity operations - returns flat structure with id merged into data
    public IEnumerable<BsonDocument> GetAll(string collection)
    {
        var col = _db.GetCollection(collection);
        return col.FindAll().ToList();
    }

    public BsonDocument? GetById(string collection, Guid id)
    {
        var col = _db.GetCollection(collection);
        return col.FindById(id);
    }

    public BsonDocument Add(string collection, JsonElement data)
    {
        var col = _db.GetCollection(collection);
        
        var doc = JsonToBson(data);
        doc["_id"] = Guid.NewGuid();
        
        col.Insert(doc);
        return doc;
    }

    public BsonDocument? Update(string collection, Guid id, JsonElement data)
    {
        var col = _db.GetCollection(collection);
        var existing = col.FindById(id);
        
        if (existing == null)
            return null;

        var doc = JsonToBson(data);
        doc["_id"] = id;
        
        col.Update(doc);
        return doc;
    }

    public bool Delete(string collection, Guid id)
    {
        var col = _db.GetCollection(collection);
        return col.Delete(id);
    }

    // Update entity data (for background jobs)
    public bool UpdateEntityData(string collection, Guid id, BsonDocument newData)
    {
        var col = _db.GetCollection(collection);
        var existing = col.FindById(id);
        
        if (existing == null)
            return false;

        newData["_id"] = id;
        return col.Update(newData);
    }

    // Schema operations
    public JsonElement? GetSchema(string collection)
    {
        return _schemas.TryGetValue(collection, out var schema) ? schema : null;
    }

    public void SetSchema(string collection, JsonElement schema)
    {
        _schemas[collection] = schema.Clone();
    }

    public bool DeleteSchema(string collection)
    {
        return _schemas.TryRemove(collection, out _);
    }

    // Background config operations
    public BackgroundJobConfig? GetBackgroundConfig(string collection)
    {
        return _backgroundConfigs.TryGetValue(collection, out var config) ? config : null;
    }

    public void SetBackgroundConfig(string collection, BackgroundJobConfig config)
    {
        _backgroundConfigs[collection] = config;
    }

    public bool DeleteBackgroundConfig(string collection)
    {
        return _backgroundConfigs.TryRemove(collection, out _);
    }

    // Reset operations
    public void ResetAll()
    {
        foreach (var name in _db.GetCollectionNames().Where(n => !n.StartsWith("_")).ToList())
        {
            _db.DropCollection(name);
        }
        _schemas.Clear();
        _backgroundConfigs.Clear();
    }

    public void ResetCollection(string collection)
    {
        var col = _db.GetCollection(collection);
        col.DeleteAll();
    }

    // Helper: Convert JsonElement to BsonDocument
    public static BsonDocument JsonToBson(JsonElement json)
    {
        var jsonString = json.GetRawText();
        return LiteDB.JsonSerializer.Deserialize(jsonString).AsDocument;
    }

    // Helper: Convert BsonDocument to JsonElement
    public static JsonElement BsonToJson(BsonDocument doc)
    {
        // Rename _id to id for API response, convert Guid to string
        if (doc.ContainsKey("_id"))
        {
            doc["id"] = doc["_id"].AsGuid.ToString();
            doc.Remove("_id");
        }
        
        var jsonString = LiteDB.JsonSerializer.Serialize(doc);
        return JsonDocument.Parse(jsonString).RootElement.Clone();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
