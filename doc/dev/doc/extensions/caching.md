# Caching

The [Hazelcast.Net.Caching](https://www.nuget.org/packages/Hazelcast.Net.Caching/) NuGet package provides an `IDistributedCache` implementation based upon the Hazelcast .NET Client. In its most basic usage, the `IDistributedCache` implementation can be instantiated explicitly with classical Hazelcast options, plus some `HazelcastCacheOptions` which specify the cache unique identifier.

The cache is implemented as a Hazelcast map, and the unique identifier corresponds to the name of the map.

The cache can be simply instantiated as:

```csharp
var hazelcastOptions = new HazelcastOptionsBuilder()....Build();
var cacheOptions = new HazelcastCacheOptions
{
    CacheUniqueIdentifier = "some-cache"
};
IDistributedCache cache = new HazelcastCache(hazelcastOptions, cacheOptions);
```

Note that the cache implementation also supports running on top of a failover client:

```csharp
var hazelcastOptions = new HazelcastFailoverOptionsBuilder()....Build();
var cacheOptions = new HazelcastCacheOptions
{
    CacheUniqueIdentifier = "some-cache"
};
IDistributedCache cache = new HazelcastCache(hazelcastOptions, cacheOptions);
```

> [!WARNING]
> Note that the failover client is only supported by Enterprise clusters.

## Dependency Injection Support

In addition, the NuGet package provides utilities for using the cache in a dependency-injection environment. Provided that Hazelcast options and cache options have been registered in a service container, then the `IDistributedCache` implementation can be registered with one single line of code:

```csharp
services.AddHazelcastOptions(...);
services.Configure<HazelcastCacheOptions>(options => options.CacheUniqueIdentifier = "some-cache");
services.AddHazelcastCache(withFailover: false); // register cache
```

Or the failover counterpart:

```csharp
services.AddHazelcastFailoverOptions(...);
services.Configure<HazelcastCacheOptions>(options => options.CacheUniqueIdentifier = "some-cache");
services.AddHazelcastCache(withFailover: true); // register cache
```

With these declarations, the Hazelcast cache implementation would be injected into any service requiring an `IDistributedCache`.

## Cache Functionality

Hazelcast's implementation of `IDistributedCache` supports all functionality of the interface. Note however that, due to the inherently asynchronous nature of the Hazelcast client, all synchronous cache operations have to be implemented over asynchronous operations using the `task.GetAwaiter().GetResult()` pattern. This pattern can cause potential threading problems, and should be avoided as much as possible. Always use the asynchronous API wherever possible.