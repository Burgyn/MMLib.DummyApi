using System.Text.Json;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Features.Custom;

public class CustomCollectionService
{
    private readonly CustomDataStore _dataStore;
    private readonly JsonSchemaValidator _validator;

    public CustomCollectionService(CustomDataStore dataStore, JsonSchemaValidator validator)
    {
        _dataStore = dataStore;
        _validator = validator;
    }

    // Collection operations
    public IEnumerable<string> GetCollections() => _dataStore.GetCollectionNames();

    public CollectionDefinition? GetDefinition(string collection) => _dataStore.GetDefinition(collection);

    public bool CollectionExists(string collection) => _dataStore.CollectionExists(collection);

    public bool IsAuthRequired(string collection) => _dataStore.IsAuthRequired(collection);

    // Entity operations - returns flat JSON with id
    public IEnumerable<JsonElement> GetAll(string collection)
    {
        return _dataStore.GetAll(collection)
            .Select(CustomDataStore.BsonToJson);
    }

    public JsonElement? GetById(string collection, Guid id)
    {
        var doc = _dataStore.GetById(collection, id);
        return doc != null ? CustomDataStore.BsonToJson(doc) : null;
    }

    public (JsonElement? Entity, List<string> Errors) Create(string collection, JsonElement data)
    {
        // Check if collection exists
        if (!_dataStore.CollectionExists(collection))
        {
            return (null, new List<string> { $"Collection '{collection}' does not exist. Create it first via POST /custom/_definitions" });
        }

        // Validate against schema if defined
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
        return (CustomDataStore.BsonToJson(doc), new List<string>());
    }

    public (JsonElement? Entity, List<string> Errors) Update(string collection, Guid id, JsonElement data)
    {
        var existing = _dataStore.GetById(collection, id);
        if (existing == null)
        {
            return (null, new List<string> { "Entity not found" });
        }

        // Validate against schema if defined
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
        return (updated != null ? CustomDataStore.BsonToJson(updated) : null, new List<string>());
    }

    public bool Delete(string collection, Guid id) => _dataStore.Delete(collection, id);

    // Background config operations (derived from definition)
    public BackgroundJobConfig? GetBackgroundConfig(string collection) => _dataStore.GetBackgroundConfig(collection);

    // Rules operations (derived from definition)
    public List<ResponseRule>? GetRules(string collection) => _dataStore.GetRules(collection);
}
