using System.Collections.Concurrent;
using System.Text.Json;
using LiteDB;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// In-memory data store for custom collections and their definitions.
/// </summary>
public class CustomDataStore : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ConcurrentDictionary<string, CollectionDefinition> _definitions = new();

    private const string DefinitionsCollection = "_definitions";

    /// <summary>
    /// Initializes a new in-memory instance of <see cref="CustomDataStore"/>.
    /// </summary>
    public CustomDataStore()
    {
        _db = new LiteDatabase("Filename=:memory:");
    }

    /// <summary>
    /// Returns all stored collection definitions.
    /// </summary>
    public IEnumerable<CollectionDefinition> GetAllDefinitions()
        => _definitions.Values;

    /// <summary>
    /// Returns the definition for the specified collection, or <c>null</c> if not found.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public CollectionDefinition? GetDefinition(string collection)
        => _definitions.TryGetValue(collection.ToLowerInvariant(), out var def) ? def : null;

    /// <summary>
    /// Saves (creates or updates) a collection definition.
    /// </summary>
    /// <param name="definition">The definition to save.</param>
    public void SaveDefinition(CollectionDefinition definition)
    {
        var key = definition.Name.ToLowerInvariant();
        _definitions[key] = definition;
    }

    /// <summary>
    /// Deletes the definition and all data for the specified collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <returns><c>true</c> if the collection was found and deleted; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Returns the names of all registered collections.
    /// </summary>
    public IEnumerable<string> GetCollectionNames()
        => _definitions.Keys;

    /// <summary>
    /// Returns <c>true</c> if a collection with the given name exists.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public bool CollectionExists(string collection)
        => _definitions.ContainsKey(collection.ToLowerInvariant());

    /// <summary>
    /// Returns all entities in the specified collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public IEnumerable<BsonDocument> GetAll(string collection)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.FindAll().ToList();
    }

    /// <summary>
    /// Returns the entity with the specified identifier, or <c>null</c> if not found.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    public BsonDocument? GetById(string collection, Guid id)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.FindById(id);
    }

    /// <summary>
    /// Inserts a new entity into the collection and returns the inserted document including the generated id.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="data">The entity data.</param>
    public BsonDocument Add(string collection, JsonElement data)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());

        var doc = JsonToBson(data);
        doc["_id"] = Guid.NewGuid();

        col.Insert(doc);
        return doc;
    }

    /// <summary>
    /// Inserts a new entity with a specific identifier into the collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The identifier to assign to the entity.</param>
    /// <param name="data">The entity data.</param>
    public BsonDocument AddWithId(string collection, Guid id, JsonElement data)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());

        var doc = JsonToBson(data);
        doc["_id"] = id;

        col.Insert(doc);
        return doc;
    }

    /// <summary>
    /// Updates an existing entity and returns the updated document, or <c>null</c> if not found.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="data">The new entity data.</param>
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

    /// <summary>
    /// Deletes the entity with the specified identifier from the collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <returns><c>true</c> if the entity was deleted; otherwise <c>false</c>.</returns>
    public bool Delete(string collection, Guid id)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        return col.Delete(id);
    }

    /// <summary>
    /// Replaces the data of an existing entity (used by background jobs).
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="newData">The new document data.</param>
    /// <returns><c>true</c> if the update succeeded; otherwise <c>false</c>.</returns>
    public bool UpdateEntityData(string collection, Guid id, BsonDocument newData)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        var existing = col.FindById(id);

        if (existing == null)
            return false;

        newData["_id"] = id;
        return col.Update(newData);
    }

    /// <summary>
    /// Returns the JSON schema for the specified collection, or <c>null</c> if not defined.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public JsonElement? GetSchema(string collection)
    {
        var def = GetDefinition(collection);
        return def?.Schema;
    }

    /// <summary>
    /// Returns the background job configuration for the specified collection, or <c>null</c> if not defined.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public BackgroundJobConfig? GetBackgroundConfig(string collection)
    {
        var def = GetDefinition(collection);
        return def?.BackgroundJob;
    }

    /// <summary>
    /// Returns the response rules for the specified collection, or <c>null</c> if not defined.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public List<ResponseRule>? GetRules(string collection)
    {
        var def = GetDefinition(collection);
        return def?.Rules;
    }

    /// <summary>
    /// Returns <c>true</c> if the specified collection requires authentication.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public bool IsAuthRequired(string collection)
    {
        var def = GetDefinition(collection);
        return def?.AuthRequired ?? false;
    }

    /// <summary>
    /// Clears all data and definitions from the store.
    /// </summary>
    public void ResetAll()
    {
        foreach (var name in _definitions.Keys.ToList())
        {
            _db.DropCollection(name);
        }
        _definitions.Clear();
    }

    /// <summary>
    /// Removes all entities from the specified collection without deleting its definition.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public void ResetCollection(string collection)
    {
        var col = _db.GetCollection(collection.ToLowerInvariant());
        col.DeleteAll();
    }

    /// <summary>
    /// Converts a <see cref="JsonElement"/> to a <see cref="BsonDocument"/>.
    /// </summary>
    /// <param name="json">The JSON element to convert.</param>
    public static BsonDocument JsonToBson(JsonElement json)
    {
        var jsonString = json.GetRawText();
        return LiteDB.JsonSerializer.Deserialize(jsonString).AsDocument;
    }

    /// <summary>
    /// Converts a <see cref="BsonDocument"/> to a <see cref="JsonElement"/>, renaming <c>_id</c> to <c>id</c>.
    /// </summary>
    /// <param name="doc">The document to convert.</param>
    public static JsonElement BsonToJson(BsonDocument doc)
    {
        if (doc.ContainsKey("_id"))
        {
            doc["id"] = doc["_id"].AsGuid.ToString();
            doc.Remove("_id");
        }

        var jsonString = LiteDB.JsonSerializer.Serialize(doc);
        return JsonDocument.Parse(jsonString).RootElement.Clone();
    }

    /// <inheritdoc/>
    public void Dispose() => _db.Dispose();
}
