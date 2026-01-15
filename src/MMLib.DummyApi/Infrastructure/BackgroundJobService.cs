using System.Collections.Concurrent;
using LiteDB;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Features.Custom.Models;
using Microsoft.Extensions.Options;

namespace MMLib.DummyApi.Infrastructure;

public class BackgroundJobService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DummyApiOptions _options;
    private readonly ConcurrentDictionary<(string Collection, Guid Id), CustomJobStatus> _customJobs = new();
    private Timer? _timer;

    public BackgroundJobService(IServiceProvider serviceProvider, IOptions<DummyApiOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ProcessJobs(object? state)
    {
        ProcessCustomJobs();
    }

    // Custom collection jobs
    public void ScheduleCustomJob(string collection, Guid entityId, int delayMs)
    {
        var completedAt = DateTime.UtcNow.AddMilliseconds(delayMs);
        _customJobs.TryAdd((collection, entityId), new CustomJobStatus(completedAt, collection, 0));
    }

    public string GetCustomJobStatus(string collection, Guid entityId)
    {
        if (_customJobs.ContainsKey((collection, entityId)))
            return "processing";
        
        return "completed";
    }

    private void ProcessCustomJobs()
    {
        var now = DateTime.UtcNow;
        var completedJobs = new List<(string Collection, Guid Id)>();

        foreach (var (key, job) in _customJobs)
        {
            if (now >= job.CompletedAt)
            {
                using var scope = _serviceProvider.CreateScope();
                var dataStore = scope.ServiceProvider.GetRequiredService<CustomDataStore>();
                
                var config = dataStore.GetBackgroundConfig(key.Collection);
                var entity = dataStore.GetById(key.Collection, key.Id);
                
                if (config != null && entity != null)
                {
                    var newData = ApplyOperationToBson(entity, config, job.SequenceIndex);
                    if (newData != null)
                    {
                        dataStore.UpdateEntityData(key.Collection, key.Id, newData);
                        
                        // For sequence operations, schedule next step if not complete
                        if (config.Operation.StartsWith("sequence:"))
                        {
                            var values = config.Operation.Substring(9).Split(',');
                            var nextIndex = job.SequenceIndex + 1;
                            if (nextIndex < values.Length)
                            {
                                var newCompletedAt = DateTime.UtcNow.AddMilliseconds(config.DelayMs);
                                _customJobs[(key.Collection, key.Id)] = new CustomJobStatus(newCompletedAt, key.Collection, nextIndex);
                                continue; // Don't remove this job yet
                            }
                        }
                    }
                }

                completedJobs.Add(key);
            }
        }

        foreach (var key in completedJobs)
        {
            _customJobs.TryRemove(key, out _);
        }
    }

    private BsonDocument? ApplyOperationToBson(BsonDocument doc, BackgroundJobConfig config, int sequenceIndex)
    {
        try
        {
            BsonValue newValue;

            if (config.Operation.StartsWith("sequence:"))
            {
                var values = config.Operation.Substring(9).Split(',');
                if (sequenceIndex < values.Length)
                {
                    var strValue = values[sequenceIndex].Trim();
                    // Try to parse as bool, int, or keep as string
                    if (bool.TryParse(strValue, out var boolVal))
                        newValue = boolVal;
                    else if (int.TryParse(strValue, out var intVal))
                        newValue = intVal;
                    else
                        newValue = strValue;
                }
                else return null;
            }
            else if (config.Operation.StartsWith("sum:"))
            {
                var path = config.Operation.Substring(4);
                newValue = CalculateSumBson(doc, path);
            }
            else if (config.Operation.StartsWith("count:"))
            {
                var path = config.Operation.Substring(6);
                newValue = CalculateCountBson(doc, path);
            }
            else if (config.Operation == "timestamp")
            {
                newValue = DateTime.UtcNow;
            }
            else if (config.Operation.StartsWith("random:"))
            {
                var parts = config.Operation.Substring(7).Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var max))
                {
                    newValue = Random.Shared.Next(min, max + 1);
                }
                else return null;
            }
            else return null;

            SetBsonValue(doc, config.FieldPath, newValue);
            return doc;
        }
        catch
        {
            return null;
        }
    }

    private static decimal CalculateSumBson(BsonDocument doc, string path)
    {
        var parts = path.Split('.');
        BsonValue current = doc;
        
        for (int i = 0; i < parts.Length - 1; i++)
        {
            current = current[parts[i]];
            if (current == null) return 0;
        }

        if (current.IsArray)
        {
            var fieldName = parts.Last();
            return current.AsArray
                .Where(item => item.AsDocument.ContainsKey(fieldName))
                .Sum(item => item[fieldName].AsDecimal);
        }

        return 0;
    }

    private static int CalculateCountBson(BsonDocument doc, string path)
    {
        BsonValue current = doc;
        foreach (var part in path.Split('.'))
        {
            current = current[part];
            if (current == null) return 0;
        }

        return current.IsArray ? current.AsArray.Count : 0;
    }

    private static void SetBsonValue(BsonDocument doc, string path, BsonValue value)
    {
        var parts = path.Split('.');
        BsonDocument current = doc;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!current.ContainsKey(parts[i]))
            {
                current[parts[i]] = new BsonDocument();
            }
            current = current[parts[i]].AsDocument;
        }

        current[parts.Last()] = value;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public record BackgroundJobStatus(DateTime CompletedAt, string Status);
public record CustomJobStatus(DateTime CompletedAt, string Collection, int SequenceIndex);
