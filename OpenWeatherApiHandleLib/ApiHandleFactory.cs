using System;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Implements the factory pattern for the 'ApiHandle' class.
	/// </summary>
	public sealed class ApiHandleFactory
	{
		/// <summary>
		/// Holds the default value for the 'GeocodingApiUrl' property.
		/// </summary>
		public const string DefaultGeocodingApiUrl = "https://api.openweathermap.org/geo/1.0/direct";

		/// <summary>
		/// Holds the default value for the 'WeatherApiUrl' property.
		/// </summary>
		public const string DefaultWeatherApiUrl = "https://api.openweathermap.org/data/2.5/weather";

		/// <summary>
		/// Holds the default value for the 'CachedWeatherLimit' property.
		/// </summary>
		public const byte DefaultCachedWeatherLimit = 10;

		/// <summary>
		/// Holds the default value for the 'WeatherRelevancePeriod' property.
		/// </summary>
		public const long DefaultWeatherRelevancePeriod = 600;

		/// <summary>
		/// Associates ApiKeys in use and corresponding not disposed 'ApiHandle' objects.
		/// </summary>
		private static Dictionary<string, ApiHandle> handles = new Dictionary<string, ApiHandle>();

		/// <summary>
		/// Is used to get current UNIX-style UTC.
		/// </summary>
		public IUnixUtcGetter UnixUtcGetter { get; }

		/// <summary>
		/// Defines the address for requesting for a location by its city name.
		/// </summary>
		public string GeocodingApiUrl { get; }

		/// <summary>
		/// Defines the address for requesting for a weather by city location.
		/// </summary>
		public string WeatherApiUrl { get; }

		/// <summary>
		/// Defines the maximum number of cached weather data.
		/// </summary>
		public byte CachedWeatherLimit { get; }

		/// <summary>
		/// Defines the period of time used to determine if weather data is relevant.
		/// Is measured in seconds.
		/// </summary>
		public long WeatherRelevancePeriod { get; }

		/// <summary>
		/// Initializes the 'ApiHandleFactory' object with default values.
		/// </summary>
		public ApiHandleFactory()
		{
			UnixUtcGetter = new SystemUnixUtcGetter();
			GeocodingApiUrl = DefaultGeocodingApiUrl;
			WeatherApiUrl = DefaultWeatherApiUrl;
			CachedWeatherLimit = DefaultCachedWeatherLimit;
			WeatherRelevancePeriod = DefaultWeatherRelevancePeriod;
		}

		/// <summary>
		/// Initializes the 'ApiHandleFactory' object with specific values.
		/// </summary>
		/// <param name="unixUtcGetter">Must not be null</param>
		/// <param name="geocodingApiUrl">Must not be null</param>
		/// <param name="weatherApiUrl">Must not be null</param>
		/// <param name="cachedWeatherLimit"></param>
		/// <param name="weatherRelevancePeriod">Must not be negative</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public ApiHandleFactory(IUnixUtcGetter unixUtcGetter, string geocodingApiUrl, string weatherApiUrl,
			byte cachedWeatherLimit, long weatherRelevancePeriod)
		{
			UnixUtcGetter = unixUtcGetter ?? throw new ArgumentNullException(nameof(unixUtcGetter));
			GeocodingApiUrl = geocodingApiUrl ?? throw new ArgumentNullException(nameof(geocodingApiUrl));
			WeatherApiUrl = weatherApiUrl ?? throw new ArgumentNullException(nameof(weatherApiUrl));
			CachedWeatherLimit = cachedWeatherLimit;
			if (weatherRelevancePeriod < 0) throw new ArgumentOutOfRangeException(nameof(weatherRelevancePeriod));
			WeatherRelevancePeriod = weatherRelevancePeriod;
		}

		/// <summary>
		/// Creates an instance of 'ApiHandle' type with the specified ApiKey and update mode.
		/// Registers the new instance with its ApiKey.
		/// </summary>
		/// <param name="apiKey">Must not be null</param>
		/// <param name="updateMode"></param>
		/// <returns>The created instance of the 'ApiHandle' type</returns>
		public ApiHandle Make(string apiKey, ApiHandleUpdateMode updateMode)
			=> Make(new HttpClient(), apiKey, updateMode);

		/// <summary>
		/// Creates an instance of 'ApiHandle' type with the specified 'HttpClient' object, ApiKey and update mode.
		/// Registers the new instance with its ApiKey in the 'handles' dictionary.
		/// </summary>
		/// <param name="httpClient">Must not be null</param>
		/// <param name="apiKey">Must not be null</param>
		/// <param name="updateMode"></param>
		/// <returns>The created instance of the 'ApiHandle' type</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="Exceptions.ApiKeyOccupiedException">If the specified ApiKey is already in use</exception>
		public ApiHandle Make(HttpClient httpClient, string apiKey, ApiHandleUpdateMode updateMode)
		{
			if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
			if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
			lock (handles)
			{
				if (handles.ContainsKey(apiKey))
					throw new Exceptions.ApiKeyOccupiedException($"ApiKey '{apiKey}' is already in use!");
				var handle = new ApiHandle(httpClient, apiKey, updateMode, this);
				handles.Add(apiKey, handle);
				return handle;
			}
		}

		/// <summary>
		/// Disposes all registered instances of the 'ApiHandle' type.
		/// </summary>
		public static void DisposeAll()
		{
			while (handles.Count > 0)
				handles.First().Value.Dispose();
		}

		/// <summary>
		/// Removes an instance of the 'ApiHandle' type from the 'handles' dictionary.
		/// </summary>
		/// <param name="handle"></param>
		internal static void RemoveOne(ApiHandle handle)
		{
			lock (handles)
				handles.Remove(handle.ApiKey);
		}
	}
}
