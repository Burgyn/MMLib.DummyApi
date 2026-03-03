using System.Collections.Concurrent;
using LiteDB;
using Microsoft.Extensions.Options;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Features.Custom.Models;

namespace MMLib.DummyApi.Infrastructure;

/// <summary>
/// Hosted service that processes scheduled background jobs for collection entities.
/// </summary>
public class BackgroundJobService(IServiceProvider serviceProvider)
    : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<(string Collection, Guid Id), CustomJobStatus> _customJobs = new();
    private Timer? _timer;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ProcessJobs(object? state) => ProcessCustomJobs();

    /// <summary>
    /// Schedules a background job to run after the specified delay for a given entity.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="delayMs">Delay in milliseconds before the job executes.</param>
    public void ScheduleCustomJob(string collection, Guid entityId, int delayMs)
    {
        var completedAt = DateTime.UtcNow.AddMilliseconds(delayMs);
        _customJobs.TryAdd((collection, entityId), new CustomJobStatus(completedAt, collection, 0));
    }

    private void ProcessCustomJobs()
    {
        var now = DateTime.UtcNow;
        List<(string Collection, Guid Id)> completedJobs = [];

        foreach (var (key, job) in _customJobs)
        {
            if (now >= job.CompletedAt)
            {
                using var scope = serviceProvider.CreateScope();
                var dataStore = scope.ServiceProvider.GetRequiredService<CustomDataStore>();

                var config = dataStore.GetBackgroundConfig(key.Collection);
                var entity = dataStore.GetById(key.Collection, key.Id);

                if (config != null && entity != null)
                {
                    var newData = ApplyOperationToBson(entity, config, job.SequenceIndex);
                    if (newData != null)
                    {
                        dataStore.UpdateEntityData(key.Collection, key.Id, newData);

                        if (config.Operation.StartsWith("sequence:"))
                        {
                            var values = config.Operation.Substring(9).Split(',');
                            var nextIndex = job.SequenceIndex + 1;
                            if (nextIndex < values.Length)
                            {
                                var newCompletedAt = DateTime.UtcNow.AddMilliseconds(config.DelayMs);
                                _customJobs[(key.Collection, key.Id)] = new CustomJobStatus(newCompletedAt, key.Collection, nextIndex);
                                continue;
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

    /// <inheritdoc/>
    public void Dispose() => _timer?.Dispose();
}

/// <summary>
/// Represents the current status of a scheduled background job.
/// </summary>
public record CustomJobStatus(DateTime CompletedAt, string Collection, int SequenceIndex);
