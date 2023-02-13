<div align="center">
  <p align="right">v[1.0.2]</p>
  <h1 align="center">OpenWeatherApiHandleLib .NET SDK</h2>
  <b align="center">Access the OpenWeather API to get current weather</b>
</div>

## Introduction
This repository is an OpenWeatherApiHandleLib .NET SDK which is used to access the [OpenWeather API](https://openweathermap.org/api) to get current weather in a specific city by its name.

## Requirements
This .NET SDK requires `.NET 6.0`

## Installation
*Note: The MIT license is not approved.*

.NET CLI
```
dotnet add package OpenWeatherApiHandleLib --version 1.0.2
```

Package Manager
```
NuGet\Install-Package OpenWeatherApiHandleLib -Version 1.0.2
```

PackageReference
```
<PackageReference Include="OpenWeatherApiHandleLib" Version="1.0.2" />
```

Paket CLI
```
paket add OpenWeatherApiHandleLib --version 1.0.2
```

Script & Interactive
```
#r "nuget: OpenWeatherApiHandleLib, 1.0.2"
```

Cake
```
// Install OpenWeatherApiHandleLib as a Cake Addin
#addin nuget:?package=OpenWeatherApiHandleLib&version=1.0.2

// Install OpenWeatherApiHandleLib as a Cake Tool
#tool nuget:?package=OpenWeatherApiHandleLib&version=1.0.2
```

## Usage
`ApiHandle` object accepts an API key for the OpenWeatherAPI on initialization.

`ApiHandle.GetWeatherInCity` method accepts the name of the city and returns information about the weather at the current moment. It returns information about the first city which was found by searching for the city name.

`ApiHandle` object stores weather information about the requested cities and if it is relevant, returns the stored value (Weather is considered to be up to date if less than `ApiHandleFactory.WeatherRelevancePeriod` seconds have passed since the measurement).

In order to save memory, `ApiHandle` object stores information for no more than `ApiHandleFactory.CachedWeatherLimit` cities at a time.

`ApiHandle` object has two types of behavior: on-demand and polling mode. In on-demand mode it updates the weather data only on user requests. In polling mode `ApiHandle` object requests new weather information for all stored locations to have zero-latency response for user requests. Mode of an `ApiHandle` object is passed as parameter on initialization.

You can work with different API keys, while creating two `ApiHandle` objects with the same key is not possible. To free used key use the `ApiHandle.Dispose` method.

### How to create an `ApiHandle` object
An `ApiHandle` object can be created with the `ApiHandleFactory.Make` method ([doc](#apihandlefactorymake-method)).

#### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory(); // The 'ApiHandleFactory' object with default values

// Creation of an 'ApiHandle' object
string apiKey = "375kgktkj44njk4n5b5b5bmkkg94nb83"; // The API key to be used by the created 'ApiHandle' object
ApiHandleUpdateMode updateMode = ApiHandleUpdateMode.OnDemand; // The mode of the updating cached weather data process
ApiHandle apiHandle = apiHandleFactory.Make(apiKey, updateMode);
```

### How to get current weather for a specific city
The `ApiHandle` class provides access to the API. The actual weather data for some specific city can be gotten with the `ApiHandle.GetWeatherInCity` method ([doc](#apihandlegetweatherincity-method)).

#### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
ApiHandle apiHandle = apiHandleFactory.Make("375kgktkj44njk4n5b5b5bmkkg94nb83", ApiHandleUpdateMode.OnDemand);

// Getting the weather data
string cityName = "London"; // The name of the city
string weatherJson = apiHandle.GetWeatherInCity(cityName); // Gotten weather is in JSON format
```

### How to handle exceptions within the polling loop
The polling loop runs in the `Threading.Task` on the background. In order to handle exceptions occurred within the polling loop, use the `ApiHandle.PollingLoopExceptionEvent` event ([doc](#apihandlepollingloopexceptionevent-event)).

#### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
ApiHandle apiHandle = apiHandleFactory.Make("375kgktkj44njk4n5b5b5bmkkg94nb83", ApiHandleUpdateMode.Polling);
object logMutex = new object();

// Adding exception handler to the 'PollingLoopExceptionEvent' event
apiHandle.PollingLoopExceptionEvent += ex => {
    // Logging the exceptions as the exception handling
    lock (logMutex) // Locking in order to avoid concurrency issues
        Console.WriteLine("An error occurred within the polling loop: {0}", ex);
};

// Doing main work
while (true)
{
    string cityName = Console.ReadLine();
    string output;
    try
    {
        string weatherJson = apiHandle.GetWeatherInCity(cityName);
        output = $"Weather gotten: {weatherJson}";
    }
    catch (Exception ex)
    {
        output = $"An error occurred during the request: {ex}";
    }
    lock (logMutex) // Locking in order to avoid concurrency issues
        Console.WriteLine(output);
}
```

#### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
ApiHandle apiHandle = apiHandleFactory.Make("375kgktkj44njk4n5b5b5bmkkg94nb83", ApiHandleUpdateMode.OnDemand);

// Getting the weather data
string cityName = "London"; // The name of the city
string weatherJson = apiHandle.GetWeatherInCity(cityName); // Gotten weather is in JSON format
```

## Documentation

### `OpenWeatherApiHandleLib` namespace
The `OpenWeatherApiHandleLib` namespace is a main namespace of the SDK.

#### Types
- `ApiHandleFactory` class
- `ApiHandle` class
- `ApiHandleUpdateMode` enum
- `IUnixUtcGetter` interface
- `SystemUnixUtcGetter` class

### `OpenWeatherApiHandleLib.Exceptions` namespace
The `OpenWeatherApiHandleLib.Exceptions` namespace contains all the exception types defined in the SDK.

#### Types
- `ApiKeyOccupiedException` class - an error caused by an attempt to create `ApiHandle` object with an API key which is already in use in another 'ApiHandle' object
- `InvalidApiKeyException` class - an error that occurs when the API does not accept used API key
- `LocationNotFoundException` class - an error that occurs if the API cannot find any location with specified city name
- `UnexpectedStatusCodeException` class - an error caused by receiving a response from the API with an unexpected status code

### `ApiHandleFactory` class
`ApiHandleFactory` type is used to create instances of `ApiHandler` type.

#### `ApiHandleFactory.UnixUtcGetter` property
Is used by `ApiHandle` objects to get current UNIX-style UTC.

Signature: `IUnixUtcGetter UnixUtcGetter { get; }`

#### `ApiHandleFactory.GeocodingApiUrl` property
Defines the address for requesting for a location by its city name.

Signature: `string GeocodingApiUrl { get; }`

#### `ApiHandleFactory.WeatherApiUrl` property
Defines the address for requesting for weather by city location.

Signature: `string WeatherApiUrl { get; }`

#### `ApiHandleFactory.CachedWeatherLimit` property
Defines the maximum number of cached weather data.

Signature: `byte CachedWeatherLimit { get; }`

#### `ApiHandleFactory.WeatherRelevancePeriod` property
Defines the period of time used to determine if weather data is relevant. Is measured in seconds.

Signature: `long WeatherRelevancePeriod { get; }`

#### Parametrized constructor
The parametrized constructor of `ApiHandleFactory` type allows to specify such common properties used by `ApiHandle` objects as `UnixUtcGetter`, `GeocodingApiUrl`, `WeatherApiUrl`, `CachedWeatherLimit`, `WeatherRelevancePeriod`.

##### Arguments
- `unixUtcGetter: IUnixUtcGetter` - must not be null
- `geocodingApiUrl: string` - must not be null; must consist of scheme, authority, path
- `weatherApiUrl: string` - must not be null; must consist of scheme, authority, path
- `cachedWeatherLimit: byte`
- `weatherRelevancePeriod: long` - is measured in seconds; must not be negative

##### Throws
- `ArgumentNullException` if any of the `unixUtcGetter`, `geocodingApiUrl`, `weatherApiUrl` arguments is `null`
- `ArgumentOutOfRangeException` if the `weatherRelevancePeriod` argument has negative value

##### Example
```cs
IUnixUtcGetter unixUtcGetter = new SystemUnixUtcGetter();
string geocodingApiUrl = "https://api.openweathermap.org/geo/1.0/direct";
string weatherApiUrl = "https://api.openweathermap.org/data/2.5/weather";
byte cachedWeatherLimit = 10;
long weatherRelevancePeriod = 600L;
ApiHandleFactory apiHandleFactory = new ApiHandleFactory(unixUtcGetter, geocodingApiUrl, weatherApiUrl, cachedWeatherLimit, weatherRelevancePeriod);
```

#### Default constructor
The default constructor of `ApiHandleFactory` type initializes the new instance's properties with default values:
- `UnixUtcGetter` = `new SystemUnixUtcGetter()`
- `GeocodingApiUrl` = `"https://api.openweathermap.org/geo/1.0/direct"`
- `WeatherApiUrl` = `"https://api.openweathermap.org/data/2.5/weather"`
- `CachedWeatherLimit` = `10`
- `WeatherRelevancePeriod` = `600L`

These default values except the value for the `UnixUtcGetter` property are stored as constants of the `ApiHandleFactory` type:
- `ApiHandleFactory.DefaultGeocodingApiUrl`
- `ApiHandleFactory.DefaultWeatherApiUrl`
- `ApiHandleFactory.DefaultCachedWeatherLimit`
- `ApiHandleFactory.DefaultWeatherRelevancePeriod`

##### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
```

#### `ApiHandleFactory.Make` method
The `ApiHandleFactory.Make` method is used to create instances of the `ApiHandle` type.

##### Arguments
- `httpClient: HttpClient` (optional) - the way to make requests to the API; if not specified, `new HttpClient()` is used as a default value
- `apiKey: string` - the ApiKey used to access the API
- `updateMode: ApiHandleUpdateMode` - defines the way of the cached weather updating process

##### Returns
- The created instance of the `ApiHandle` type

##### Throws
- `ArgumentNullException` if any of `httpClient`, `apiKey` arguments is `null`
- `Exceptions.ApiKeyOccupiedException` if the specified API key is already in use

##### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
string apiKey = "375kgktkj44njk4n5b5b5bmkkg94nb83";
ApiHandleUpdateMode updateMode = ApiHandleUpdateMode.OnDemand;
ApiHandle apiHandle = apiHandleFactory.Make(apiKey, updateMode);
```

#### `ApiHandleFactory.DisposeAll` method
The `ApiHandleFactory.DisposeAll` method is used to dispose all registered instances of the `ApiHandle` type.

##### Example
```cs
ApiHandleFactory apiHandleFactory = new ApiHandleFactory();
ApiHandle apiHandle = apiHandleFactory.Make("375kgktkj44njk4n5b5b5bmkkg94nb83", ApiHandleUpdateMode.OnDemand);
Console.WriteLine(apiHandle.Disposed); // False
apiHandleFactory.DisposeAll();
Console.WriteLine(apiHandle.Disposed); // True
```

### `ApiHandle` class
`ApiHandleFactory` type is used to access the API. An object can store retrieved weather data for requested city locations.

In the `ApiHandleUpdateMode.OnDemand` update mode the object retrieves weather data only on user request.

In the `ApiHandleUpdateMode.Polling` update mode the object starts the polling loop on the background. The polling loop updates all the cached data once per `WeatherRelevancePeriod` seconds.

#### `ApiHandle.ExceptionHandler` delegate
The `ApiHandle.ExceptionHandler` delegate represents a delegate used to handle an exception.

Signature: `void ExceptionHandler(Exception ex)`

#### `ApiHandle.PollingLoopExceptionEvent` event
Is called from the polling loop when catching an exception within the loop.

Delegate: `ApiHandle.ExceptionHandler`

#### `ApiHandle.HttpClient` property
Is used to make requests to the API.

Signature: `HttpClient HttpClient { get; }`

#### `ApiHandle.ApiKey` property
Is used to authorize API calls.

Signature: `string ApiKey { get; }`

#### `ApiHandle.UpdateMode` property
Affects on the cached weather data updating process.

Signature: `ApiHandleUpdateMode UpdateMode { get; }`

#### `ApiHandle.Factory` property
Provides such common properties used by an `ApiHandle` object as `UnixUtcGetter`, `GeocodingApiUrl`, `WeatherApiUrl`, `CachedWeatherLimit`, `WeatherRelevancePeriod`.

Signature: `ApiHandleFactory Factory { get; }`

#### `ApiHandle.Disposed` property
The flag that indicates if an `ApiHandle` object is disposed.

Signature: `bool Disposed { get; private set; }`

#### `ApiHandle.GetWeatherInCity` method
The `ApiHandle.GetWeatherInCity` method is used to get relevant weather data at a specific city location.

The method makes an API call to find the location by the specified city name and selects the first found location, then it makes an API call to get the weather data at the location.

The method caches retrieved data. If cache record number exceeds the limit (`CachedWeatherLimit`), the method removes one cache record which last user request datetime is the oldest one.

If the requested city location is cached, the method returns the cached weather data in case it is relevant (the time since its measurement ≤ `WeatherRelevancePeriod`) or makes an API call to get the weather data at the cached location, updates the cached weather data and returns it in other case.

##### Arguments
- `cityName: string` - the name of the city to get the weather for

##### Returns
- Relevant weather data for the specified location in JSON format

##### Throws
- `ArgumentNullException` if the `cityName` argument is `null`
- `InvalidOperationException` if the object is disposed
- `Exceptions.InvalidApiKeyException` if the API's server does not accept the API key
- `Exceptions.LocationNotFoundException` if the API cannot find any location with specified city name
- `Exceptions.UnexpectedStatusCodeException` if the method cannot handle status code of received response
- `HttpRequestException` if some error occurs during an API call

##### Example
```cs
string cityName = "London";
string weatherJson = apiHandle.GetWeatherInCity(cityName);
```

#### `ApiHandle.Dispose` method
The `ApiHandle.Dispose` method disposes the `ApiHandle` object: frees the API key and sets `Disposed` flag-property.

The method is automatically called from the destructor.

##### Example
```cs
Console.WriteLine(apiHandle.Disposed); // False
apiHandleFactory.DisposeAll();
Console.WriteLine(apiHandle.Disposed); // True
```

### `ApiHandleUpdateMode` enum
The `ApiHandleUpdateMode` enum represents the mode of cached weather data updating for the `ApiHandle` class.

#### Values
- `OnDemand` - cached weather data is updated on user requests if the data is not relevant
- `Polling` - cached weather data is updated within the polling loop

### `IUnixUtcGetter` interface
The `IUnixUtcGetter` interface defines a source of current UNIX-style UTC.

#### `IUnixUtcGetter.Seconds` property
The UNIX-style UTC in seconds.

Signature: `long Seconds { get; }`

### `SystemUnixUtcGetter` class
The `SystemUnixUtcGetter` class represents a source of current UNIX-style UTC based on `System.DateTime`.

#### `SystemUnixUtcGetter.Seconds` property
The UNIX-style UTC in seconds.

Signature: `long Seconds { get; }`
