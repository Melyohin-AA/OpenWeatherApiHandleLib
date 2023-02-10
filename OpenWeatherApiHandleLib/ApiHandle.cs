using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenWeatherApiHandleLib
{
	public sealed class ApiHandle : IDisposable
	{
		private static CultureInfo culture = new CultureInfo("en-UK");

		private readonly Dictionary<string, WeatherApiHandle.Weather> cachedWeather;
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
			cachedWeather = new Dictionary<string, WeatherApiHandle.Weather>(Factory.CachedWeatherLimit);
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
							cachedWeather[pair.Key] = RequestForWeather(pair.Value.coord);
					}
				}
				pollingCancellation.Dispose();
			}, cancellationToken);
		}

		public string GetWeatherInCity(string cityName)
		{
			if (cityName == null) throw new ArgumentNullException(nameof(cityName));
			if (Disposed) throw new InvalidOperationException("Attempt of using disposed ApiHandle object!");
			WeatherApiHandle.Weather weather;
			lock (cachedWeather)
			{
				if (cachedWeather.ContainsKey(cityName))
				{
					weather = cachedWeather[cityName];
					if ((UpdateMode == ApiHandleUpdateMode.OnDemand) && !IsDtRelevant(weather.dt))
					{
						weather = RequestForWeather(weather.coord);
						cachedWeather[cityName] = weather;
					}
				}
				else
				{
					weather = RequestForWeather(cityName);
					if (Factory.CachedWeatherLimit > 0)
					{
						if (cachedWeather.Count == Factory.CachedWeatherLimit)
							RemoveOldestCachedWeather();
						cachedWeather.Add(cityName, weather);
					}
				}
			}
			return weather.ToJson(culture);
		}

		private WeatherApiHandle.Weather RequestForWeather(string cityName)
			=> RequestForWeather(RequestForCityLocation(cityName));
		private WeatherApiHandle.Weather RequestForWeather(WeatherApiHandle.WeatherCoord cityLocation)
			=> WeatherApiHandle.GetWeatherAtLocation(cityLocation, ApiKey, Factory.WeatherApiUrl, HttpClient, culture);
		private WeatherApiHandle.WeatherCoord RequestForCityLocation(string cityName)
			=> GeocodingApiHandle.GetCityLocation(cityName, ApiKey, Factory.GeocodingApiUrl, HttpClient, culture);

		private void RemoveOldestCachedWeather()
		{
			if (cachedWeather.Count == 0) throw new InvalidOperationException();
			string? oldestCityName = null;
			long oldestDT = long.MaxValue;
			foreach (var pair in cachedWeather)
			{
				if (pair.Value.dt >= oldestDT) continue;
				oldestDT = pair.Value.dt;
				oldestCityName = pair.Key;
			}
			cachedWeather.Remove(oldestCityName);
		}

		private bool IsDtRelevant(long dt) => Factory.UnixUtcGetter.Seconds - dt <= Factory.WeatherRelevancePeriod;

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
	}
}
