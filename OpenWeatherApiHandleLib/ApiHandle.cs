using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenWeatherApiHandleLib
{
	public sealed class ApiHandle : IDisposable
	{
		private static CultureInfo culture = new CultureInfo("en-UK");

		private readonly Dictionary<string, UserRequest> cachedWeather;
		private CancellationTokenSource? pollingCancellation;

		public HttpClient HttpClient { get; }
		public string ApiKey { get; }
		public ApiHandleUpdateMode UpdateMode { get; }
		public ApiHandleFactory Factory { get; }

		public bool Disposed { get; internal set; }
		private readonly object disposeMutex = new object();

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
		~ApiHandle() => Dispose();

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
				pollingCancellation.Dispose();
			}, cancellationToken);
		}

		public string GetWeatherInCity(string cityName)
		{
			if (cityName == null) throw new ArgumentNullException(nameof(cityName));
			if (Disposed) throw new InvalidOperationException("Attempt of using disposed ApiHandle object!");
			long userRequestDt = Factory.UnixUtcGetter.Seconds;
			WeatherApiHandle.Weather weather;
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

		private bool IsDtRelevant(long dt) => Factory.UnixUtcGetter.Seconds - dt <= Factory.WeatherRelevancePeriod;

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

		private WeatherApiHandle.Weather RequestForWeather(string cityName)
			=> RequestForWeather(RequestForCityLocation(cityName));
		private WeatherApiHandle.Weather RequestForWeather(WeatherApiHandle.WeatherCoord cityLocation)
			=> WeatherApiHandle.GetWeatherAtLocation(cityLocation, ApiKey, Factory.WeatherApiUrl, HttpClient, culture);
		private WeatherApiHandle.WeatherCoord RequestForCityLocation(string cityName)
			=> GeocodingApiHandle.GetCityLocation(cityName, ApiKey, Factory.GeocodingApiUrl, HttpClient, culture);

		public void Dispose()
		{
			if (Disposed) return;
			lock (disposeMutex)
			{
				ApiHandleFactory.DisposeOne(this);
				pollingCancellation?.Cancel();
				Disposed = true;
			}
		}

		private sealed class UserRequest
		{
			public long Dt { get; set; }
			public WeatherApiHandle.Weather Weather { get; set; }

			public UserRequest(long dt, WeatherApiHandle.Weather weather)
			{
				Dt = dt;
				Weather = weather;
			}
		}
	}
}
