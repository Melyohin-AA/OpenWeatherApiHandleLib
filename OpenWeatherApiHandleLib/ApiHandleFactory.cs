using System;

namespace OpenWeatherApiHandleLib
{
	public class ApiHandleFactory
	{
		public const string DefaultGeocodingApiUrl = "https://api.openweathermap.org/geo/1.0/direct";
		public const string DefaultWeatherApiUrl = "https://api.openweathermap.org/data/2.5/weather";
		public const ushort DefaultCachedWeatherLimit = 10;
		public const long DefaultWeatherRelevancePeriod = 600;

		private static Dictionary<string, ApiHandle> handles = new Dictionary<string, ApiHandle>();

		public IUnixUtcGetter UnixUtcGetter { get; }
		public string GeocodingApiUrl { get; }
		public string WeatherApiUrl { get; }
		public ushort CachedWeatherLimit { get; }
		public long WeatherRelevancePeriod { get; }

		public ApiHandleFactory()
		{
			UnixUtcGetter = new SystemUnixUtcGetter();
			GeocodingApiUrl = DefaultGeocodingApiUrl;
			WeatherApiUrl = DefaultWeatherApiUrl;
			CachedWeatherLimit = DefaultCachedWeatherLimit;
			WeatherRelevancePeriod = DefaultWeatherRelevancePeriod;
		}
		public ApiHandleFactory(IUnixUtcGetter unixUtcGetter, string geocodingApiUrl, string weatherApiUrl,
			ushort cachedWeatherLimit, long weatherRelevancePeriod)
		{
			UnixUtcGetter = unixUtcGetter ?? throw new ArgumentNullException(nameof(unixUtcGetter));
			GeocodingApiUrl = geocodingApiUrl ?? throw new ArgumentNullException(nameof(geocodingApiUrl));
			WeatherApiUrl = weatherApiUrl ?? throw new ArgumentNullException(nameof(weatherApiUrl));
			CachedWeatherLimit = cachedWeatherLimit;
			if (weatherRelevancePeriod < 0) throw new ArgumentOutOfRangeException(nameof(weatherRelevancePeriod));
			WeatherRelevancePeriod = weatherRelevancePeriod;
		}

		public ApiHandle Make(string apiKey, ApiHandleUpdateMode updateMode)
			=> Make(new HttpClient(), apiKey, updateMode);
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

		public static void DisposeAll()
		{
			while (handles.Count > 0)
				handles.First().Value.Dispose();
		}

		internal static void DisposeOne(ApiHandle handle)
		{
			lock (handles)
			{
				handles.Remove(handle.ApiKey);
			}
		}
	}
}
