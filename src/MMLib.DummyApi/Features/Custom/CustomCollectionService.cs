using System.Text.Json;
using LiteDB;
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

    public bool DeleteCollection(string collection) => _dataStore.DeleteCollection(collection);

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
        var (isValid, errors) = _validator.Validate(collection, data);
        if (!isValid)
        {
            return (null, errors);
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

        var (isValid, errors) = _validator.Validate(collection, data);
        if (!isValid)
        {
            return (null, errors);
        }

        var updated = _dataStore.Update(collection, id, data);
        return (updated != null ? CustomDataStore.BsonToJson(updated) : null, new List<string>());
    }

    public bool Delete(string collection, Guid id) => _dataStore.Delete(collection, id);

    // Schema operations
    public JsonElement? GetSchema(string collection) => _dataStore.GetSchema(collection);

    public (bool Success, string? Error) SetSchema(string collection, JsonElement schema)
    {
        var (isValid, error) = _validator.ValidateSchema(schema);
        if (!isValid)
        {
            return (false, error);
        }

        _dataStore.SetSchema(collection, schema);
        return (true, null);
    }

    public bool DeleteSchema(string collection) => _dataStore.DeleteSchema(collection);

    // Background config operations
    public BackgroundJobConfig? GetBackgroundConfig(string collection) => _dataStore.GetBackgroundConfig(collection);

    public void SetBackgroundConfig(string collection, BackgroundJobConfig config) 
        => _dataStore.SetBackgroundConfig(collection, config);

    public bool DeleteBackgroundConfig(string collection) => _dataStore.DeleteBackgroundConfig(collection);
}
