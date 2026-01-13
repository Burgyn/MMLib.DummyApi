using System.Collections.Concurrent;

namespace MMLib.DummyApi.Infrastructure;

public class DataStore<TKey, TEntity> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TEntity> _data = new();
    private readonly Func<TEntity, TKey> _keySelector;
    private readonly List<TEntity> _initialData = new();

    public DataStore(Func<TEntity, TKey> keySelector)
    {
        _keySelector = keySelector;
    }

    public IEnumerable<TEntity> GetAll() => _data.Values;

    public TEntity? GetById(TKey id) => _data.TryGetValue(id, out var entity) ? entity : default;

    public TEntity Add(TEntity entity)
    {
        var key = _keySelector(entity);
        _data.TryAdd(key, entity);
        return entity;
    }

    public bool Update(TKey id, TEntity entity)
    {
        if (!_data.ContainsKey(id))
            return false;

        _data[id] = entity;
        return true;
    }

    public bool Delete(TKey id) => _data.TryRemove(id, out _);

    public void Reset()
    {
        _data.Clear();
        foreach (var item in _initialData)
        {
            var key = _keySelector(item);
            _data.TryAdd(key, item);
        }
    }

    public void Seed(IEnumerable<TEntity> initialData)
    {
        _initialData.Clear();
        _initialData.AddRange(initialData);
        Reset();
    }

    public void Clear()
    {
        _data.Clear();
        _initialData.Clear();
    }
}
