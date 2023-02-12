using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Represents an interface for the API.
	/// </summary>
	public sealed class ApiHandle : IDisposable
	{
		/// <summary>
		/// Is used to specify the format of float number text representation.
		/// </summary>
		private static CultureInfo culture = new CultureInfo("en-UK");

		/// <summary>
		/// The collection used to cache weather data.
		/// Stores such info as city name, datetime of last user request for the city, weather data in the city.
		/// </summary>
		private readonly Dictionary<string, UserRequest> cachedWeather;

		/// <summary>
		/// Is used to stop the weather polling loop.
		/// </summary>
		private CancellationTokenSource? pollingCancellation;

		/// <summary>
		/// Is called from the polling loop when catching an exception within the loop.
		/// </summary>
		public event ExceptionHandler PollingLoopExceptionEvent;

		/// <summary>
		/// Is used to make requests to the API.
		/// </summary>
		public HttpClient HttpClient { get; }

		/// <summary>
		/// Is used to authorize requests to the API.
		/// </summary>
		public string ApiKey { get; }

		/// <summary>
		/// Affects on the cached weather data updating process.
		/// </summary>
		public ApiHandleUpdateMode UpdateMode { get; }

		/// <summary>
		/// Provides such common properties used by 'ApiHandle' object as
		/// 'UnixUtcGetter', 'GeocodingApiUrl', 'WeatherApiUrl', 'CachedWeatherLimit', 'WeatherRelevancePeriod'.
		/// </summary>
		public ApiHandleFactory Factory { get; }

		/// <summary>
		/// The flag that indicates if an 'ApiHandle' object is disposed.
		/// </summary>
		public bool Disposed { get; internal set; }

		/// <summary>
		/// Is used to lock the disposing process.
		/// </summary>
		private readonly object disposeMutex = new object();

		/// <summary>
		/// Initializes an 'ApiHandle' object.
		/// Calls 'StartPollingCycle' method if 'UpdateMode' == 'Polling'.
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="apiKey"></param>
		/// <param name="updateMode"></param>
		/// <param name="factory"></param>
		internal ApiHandle(HttpClient httpClient, string apiKey, ApiHandleUpdateMode updateMode,
			ApiHandleFactory factory)
		{
			HttpClient = httpClient;
			ApiKey = apiKey;
			UpdateMode = updateMode;
			Factory = factory;
			cachedWeather = new Dictionary<string, UserRequest>(Factory.CachedWeatherLimit);
			if (UpdateMode == ApiHandleUpdateMode.Polling)
				StartPollingCycle();
		}

		/// <summary>
		/// Calls the disposing process on object distruction.
		/// </summary>
		~ApiHandle() => Dispose();

		/// <summary>
		/// Starts the weather polling loop.
		/// The polling loop updates weather data with the period equal to 'WeatherRelevancePeriod'.
		/// </summary>
		private void StartPollingCycle()
		{
			pollingCancellation = new CancellationTokenSource();
			var cancellationToken = pollingCancellation.Token;
			Task.Run(() => {
				long msPeriod = Factory.WeatherRelevancePeriod * 1000;
				int delay;
				var sw = new Stopwatch();
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						sw.Stop();
						checked
						{
							delay = (int)(msPeriod - sw.ElapsedMilliseconds);
						}
						if (delay > 0) Thread.Sleep(delay);
						if (cancellationToken.IsCancellationRequested) break;
						sw.Restart();
						lock (cachedWeather)
						{
							foreach (var pair in cachedWeather.ToArray())
								cachedWeather[pair.Key].Weather = RequestForWeather(pair.Value.Weather.coord);
						}
					}
					catch (Exception ex)
					{
						PollingLoopExceptionEvent?.Invoke(ex);
					}
				}
				pollingCancellation.Dispose();
			}, cancellationToken);
		}

		/// <summary>
		/// Retrieves weather data for the specified city on user request.
		/// Manages the weather cache.
		/// </summary>
		/// <param name="cityName">Must not be null</param>
		/// <returns>Relevant weather data for the specified location in JSON format</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the object is disposed</exception>
		public string GetWeatherInCity(string cityName)
		{
			if (cityName == null) throw new ArgumentNullException(nameof(cityName));
			if (Disposed) throw new InvalidOperationException("Attempt of using disposed ApiHandle object!");
			long userRequestDt = Factory.UnixUtcGetter.Seconds;
			WeatherCaller.Weather weather;
			lock (cachedWeather)
			{
				if (cachedWeather.ContainsKey(cityName))
				{
					UserRequest userRequest = cachedWeather[cityName];
					userRequest.Dt = userRequestDt;
					weather = userRequest.Weather;
					if ((UpdateMode == ApiHandleUpdateMode.OnDemand) && !IsDtRelevant(weather.dt))
					{
						weather = RequestForWeather(weather.coord);
						userRequest.Weather = weather;
					}
				}
				else
				{
					weather = RequestForWeather(cityName);
					if (Factory.CachedWeatherLimit > 0)
					{
						if (cachedWeather.Count == Factory.CachedWeatherLimit)
							RemoveOldestCachedWeather();
						cachedWeather.Add(cityName, new UserRequest(userRequestDt, weather));
					}
				}
			}
			return weather.ToJson(culture);
		}

		/// <summary>
		/// Determines if the specified datetime is relevant.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns>'true' if the specified datetime is relevant, otherwise 'false'</returns>
		private bool IsDtRelevant(long dt) => Factory.UnixUtcGetter.Seconds - dt <= Factory.WeatherRelevancePeriod;

		/// <summary>
		/// Removes one cache data record
		/// which last user request datatime is the oldest one.
		/// </summary>
		/// <exception cref="InvalidOperationException">If no chached weather data</exception>
		private void RemoveOldestCachedWeather()
		{
			if (cachedWeather.Count == 0) throw new InvalidOperationException();
			string? oldestCityName = null;
			long oldestRequestDt = long.MaxValue;
			foreach (var pair in cachedWeather)
			{
				if (pair.Value.Dt >= oldestRequestDt) continue;
				oldestRequestDt = pair.Value.Dt;
				oldestCityName = pair.Key;
			}
			cachedWeather.Remove(oldestCityName);
		}

		/// <summary>
		/// Requests for the weather at the location with the specified city name.
		/// </summary>
		/// <param name="cityName"></param>
		/// <returns>Weather data at location with the specified city name</returns>
		private WeatherCaller.Weather RequestForWeather(string cityName)
			=> RequestForWeather(RequestForCityLocation(cityName));

		/// <summary>
		/// Requests for the weather at the specified location.
		/// </summary>
		/// <param name="cityLocation"></param>
		/// <returns>Weather data at the specified location</returns>
		private WeatherCaller.Weather RequestForWeather(WeatherCaller.WeatherCoord cityLocation)
			=> WeatherCaller.GetWeatherAtLocation(cityLocation, ApiKey, Factory.WeatherApiUrl, HttpClient, culture);

		/// <summary>
		/// Searches for the location with the specified city name.
		/// </summary>
		/// <param name="cityName"></param>
		/// <returns>Geographical coordinates of the location with the specified city name</returns>
		private WeatherCaller.WeatherCoord RequestForCityLocation(string cityName)
			=> GeocodingCaller.GetCityLocation(cityName, ApiKey, Factory.GeocodingApiUrl, HttpClient, culture);

		/// <summary>
		/// Disposes the object:
		/// frees the ApiKey,
		/// requests for the stop of the polling loop if it works,
		/// sets 'Disposed' flag.
		/// </summary>
		public void Dispose()
		{
			if (Disposed) return;
			lock (disposeMutex)
			{
				ApiHandleFactory.RemoveOne(this);
				pollingCancellation?.Cancel();
				Disposed = true;
			}
		}

		/// <summary>
		/// Represents a delegate used to handle an exception.
		/// </summary>
		/// <param name="ex"></param>
		public delegate void ExceptionHandler(Exception ex);

		/// <summary>
		/// Represents a user request for weather in a specific city.
		/// </summary>
		private sealed class UserRequest
		{
			/// <summary>
			/// The datetime of the latest user request for the weather in the specific city.
			/// </summary>
			public long Dt { get; set; }

			/// <summary>
			/// The cached weather data.
			/// </summary>
			public WeatherCaller.Weather Weather { get; set; }

			/// <summary>
			/// Initializes the 'UserRequest' object.
			/// </summary>
			/// <param name="dt"></param>
			/// <param name="weather"></param>
			public UserRequest(long dt, WeatherCaller.Weather weather)
			{
				Dt = dt;
				Weather = weather;
			}
		}
	}
}
