using System.Collections.Concurrent;
using System.Text.Json;
using LiteDB;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

public class CustomDataStore : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ConcurrentDictionary<string, CollectionDefinition> _definitions = new();

    private const string DefinitionsCollection = "_definitions";

    public CustomDataStore()
    {
        _db = new LiteDatabase("Filename=:memory:");
    }

    // Collection definition operations
    public IEnumerable<CollectionDefinition> GetAllDefinitions()
        => _definitions.Values;

    public CollectionDefinition? GetDefinition(string collection)
        => _definitions.TryGetValue(collection.ToLowerInvariant(), out var def) ? def : null;

    public void SaveDefinition(CollectionDefinition definition)
    {
        var key = definition.Name.ToLowerInvariant();
        _definitions[key] = definition;
    }

    public bool DeleteDefinition(string collection)
    {
        var key = collection.ToLowerInvariant();
        if (_definitions.TryRemove(key, out _))
        {
            _db.DropCollection(key);
            return true;
        }
        return false;
    }

    // Collection operations
    public IEnumerable<string> GetCollectionNames() 
        => _definitions.Keys;

    public bool CollectionExists(string collection) 
        => _definitions.ContainsKey(collection.ToLowerInvariant());

    // Entity operations - returns flat structure with id merged into data
    public IEnumerable<BsonDocument> GetAll(string collection)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.FindAll().ToList();
    }

    public BsonDocument? GetById(string collection, Guid id)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.FindById(id);
    }

    public BsonDocument Add(string collection, JsonElement data)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        
        var doc = JsonToBson(data);
        doc["_id"] = Guid.NewGuid();
        
        col.Insert(doc);
        return doc;
    }

    public BsonDocument AddWithId(string collection, Guid id, JsonElement data)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        
        var doc = JsonToBson(data);
        doc["_id"] = id;
        
        col.Insert(doc);
        return doc;
    }

    public BsonDocument? Update(string collection, Guid id, JsonElement data)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
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
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.Delete(id);
    }

    // Update entity data (for background jobs)
    public bool UpdateEntityData(string collection, Guid id, BsonDocument newData)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        var existing = col.FindById(id);
        
        if (existing == null)
            return false;

        newData["_id"] = id;
        return col.Update(newData);
    }

    // Schema operations (derived from definition)
    public JsonElement? GetSchema(string collection)
    {
        var def = GetDefinition(collection);
        return def?.Schema;
    }

    // Background config operations (derived from definition)
    public BackgroundJobConfig? GetBackgroundConfig(string collection)
    {
        var def = GetDefinition(collection);
        return def?.BackgroundJob;
    }

    // Rules operations (derived from definition)
    public List<ResponseRule>? GetRules(string collection)
    {
        var def = GetDefinition(collection);
        return def?.Rules;
    }

    // Auth required check
    public bool IsAuthRequired(string collection)
    {
        var def = GetDefinition(collection);
        return def?.AuthRequired ?? false;
    }

    // Reset operations
    public void ResetAll()
    {
        foreach (var name in _definitions.Keys.ToList())
        {
            _db.DropCollection(name);
        }
        _definitions.Clear();
    }

    public void ResetCollection(string collection)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
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
