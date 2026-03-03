using System.Text.Json;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Service for managing entities and definitions within custom collections.
/// </summary>
public class CustomCollectionService(CustomDataStore dataStore, JsonSchemaValidator validator)
{
    private readonly CustomDataStore _dataStore = dataStore;
    private readonly JsonSchemaValidator _validator = validator;

    /// <summary>
    /// Returns the names of all registered collections.
    /// </summary>
    public IEnumerable<string> GetCollections() => _dataStore.GetCollectionNames();

    /// <summary>
    /// Returns the definition for the specified collection, or <c>null</c> if not found.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public CollectionDefinition? GetDefinition(string collection) => _dataStore.GetDefinition(collection);

    /// <summary>
    /// Returns <c>true</c> if the specified collection exists.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public bool CollectionExists(string collection) => _dataStore.CollectionExists(collection);

    /// <summary>
    /// Returns <c>true</c> if the specified collection requires authentication.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public bool IsAuthRequired(string collection) => _dataStore.IsAuthRequired(collection);

    /// <summary>
    /// Returns all entities in the specified collection as JSON.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public IEnumerable<JsonElement> GetAll(string collection)
    {
        return _dataStore.GetAll(collection)
            .Select(CustomDataStore.BsonToJson);
    }

    /// <summary>
    /// Returns the entity with the given identifier, or <c>null</c> if not found.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    public JsonElement? GetById(string collection, Guid id)
    {
        var doc = _dataStore.GetById(collection, id);
        return doc != null ? CustomDataStore.BsonToJson(doc) : null;
    }

    /// <summary>
    /// Creates a new entity in the specified collection after validating against the schema.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="data">The entity data.</param>
    public (JsonElement? Entity, List<string> Errors) Create(string collection, JsonElement data)
    {
        if (!_dataStore.CollectionExists(collection))
        {
            return (null, [$"Collection '{collection}' does not exist. Create it first via POST /custom/_definitions"]);
        }

        var schema = _dataStore.GetSchema(collection);
        if (schema != null)
        {
            var (isValid, errors) = _validator.Validate(collection, data, schema.Value);
            if (!isValid)
            {
                return (null, errors);
            }
        }

        var doc = _dataStore.Add(collection, data);
        return (CustomDataStore.BsonToJson(doc), []);
    }

    /// <summary>
    /// Updates an existing entity in the specified collection after validating against the schema.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="data">The new entity data.</param>
    public (JsonElement? Entity, List<string> Errors) Update(string collection, Guid id, JsonElement data)
    {
        var existing = _dataStore.GetById(collection, id);
        if (existing == null)
        {
            return (null, ["Entity not found"]);
        }

        var schema = _dataStore.GetSchema(collection);
        if (schema != null)
        {
            var (isValid, errors) = _validator.Validate(collection, data, schema.Value);
            if (!isValid)
            {
                return (null, errors);
            }
        }

        var updated = _dataStore.Update(collection, id, data);
        return (updated != null ? CustomDataStore.BsonToJson(updated) : null, []);
    }

    /// <summary>
    /// Deletes the entity with the given identifier from the specified collection.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <returns><c>true</c> if the entity was deleted; otherwise <c>false</c>.</returns>
    public bool Delete(string collection, Guid id) => _dataStore.Delete(collection, id);

    /// <summary>
    /// Returns the background job configuration for the specified collection, or <c>null</c> if not defined.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public BackgroundJobConfig? GetBackgroundConfig(string collection) => _dataStore.GetBackgroundConfig(collection);

    /// <summary>
    /// Returns the response rules for the specified collection, or <c>null</c> if not defined.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    public List<ResponseRule>? GetRules(string collection) => _dataStore.GetRules(collection);
}
