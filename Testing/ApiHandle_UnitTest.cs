using System;
using System.Globalization;

namespace OpenWeatherApiHandleLib.Testing
{
	[TestClass]
	public class ApiHandle_UnitTest
	{
		private const string geocodingRequestLocalPath = "/geo/1.0/direct";
		private const string weatherRequestLocalPath = "/data/2.5/weather";

		private readonly static CultureInfo culture = new CultureInfo("en-UK");

		private static string BuildGeocodingResponse(string city, double lat, double lon, string country, string state)
		{
			string latStr = lat.ToString(culture), lonStr = lon.ToString(culture);
			return $"[{{\"name\":\"{city}\",\"local_names\":{{}},\"lat\":{latStr},\"lon\":{lonStr},\"country\":\"{country}\",\"state\":\"{state}\"}}]";
		}
		private static string BuildWeatherResponse(double lat, double lon, long dt, string city, string country)
		{
			string latStr = lat.ToString(culture), lonStr = lon.ToString(culture);
			return $"{{\"coord\":{{\"lon\":{lonStr},\"lat\":{latStr}}},\"weather\":[{{\"id\":802,\"main\":\"Clouds\",\"description\":\"scattered clouds\",\"icon\":\"03d\"}}],\"base\":\"stations\",\"main\":{{\"temp\":281.75,\"feels_like\":279.92,\"temp_min\":279.7,\"temp_max\":283.15,\"pressure\":1032,\"humidity\":77}},\"visibility\":10000,\"wind\":{{\"speed\":3.09,\"deg\":300}},\"clouds\":{{\"all\":40}},\"dt\":{dt},\"sys\":{{\"type\":2,\"id\":2075535,\"country\":\"{country}\",\"sunrise\":1675927583,\"sunset\":1675962189}},\"timezone\":0,\"id\":2643743,\"name\":\"{city}\",\"cod\":200}}";
		}
		private static string BuildWeatherJson(double lat, double lon, long dt, string city)
		{
			string latStr = lat.ToString(culture), lonStr = lon.ToString(culture);
			return $"{{\"coord\":{{\"lat\":{latStr},\"lon\":{lonStr}}},\"weather\":[{{\"id\":802,\"main\":\"Clouds\",\"description\":\"scattered clouds\"}}],\"main\":{{\"temp\":281.75,\"feels_like\":279.92,\"pressure\":1032,\"humidity\":77}},\"visibility\":10000,\"wind\":{{\"speed\":3.09,\"deg\":300}},\"dt\":{dt},\"sys\":{{\"sunrise\":1675927583,\"sunset\":1675962189}},\"timezone\":0,\"name\":\"{city}\"}}";
		}

		private static ApiHandle CreateApiHandleForStartPollingCycle(string city,
			Action? handleGeocodingRequest, Action? handleWeatherRequest)
		{
			const string country = "GB", state = "England";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			const double weatherLat = 51.5074, weatherLon = -0.1278;
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.Polling;
			var unixUtcGetter = new SystemUnixUtcGetter();
			const long weatherRelevancePeriod = 1L;
			ApiHandleFactory apiHandleFactory = new ApiHandleFactory(unixUtcGetter,
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, weatherRelevancePeriod);
			string geocodingResponse = BuildGeocodingResponse(city, geoLat, geoLon, country, state);
			string weatherResponse = BuildWeatherResponse(weatherLat, weatherLon, unixUtcGetter.Seconds, city, country);
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request => {
				switch (request.RequestUri?.LocalPath)
				{
					case geocodingRequestLocalPath:
						handleGeocodingRequest?.Invoke();
						return geocodingResponse;
					case weatherRequestLocalPath:
						handleWeatherRequest?.Invoke();
						return weatherResponse;
				}
				return null;
			}));
			return apiHandleFactory.Make(httpClient, apiKey, updateMode);
		}

		[TestMethod]
		public void StartPollingCycle_CyclePeriodLowerBound()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			int weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForStartPollingCycle(city, null, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city);
			Thread.Sleep(950);
			apiHandle.Dispose();
			//
			Assert.AreEqual(1, weatherRequestCount);
		}
		[TestMethod]
		public void StartPollingCycle_CyclePeriodUpperBound()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			int weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForStartPollingCycle(city, null, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city);
			Thread.Sleep(1050);
			apiHandle.Dispose();
			//
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void StartPollingCycle_GeocodingRequestCount()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			int geocodingRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForStartPollingCycle(city, () => geocodingRequestCount++, null);
			//
			apiHandle.GetWeatherInCity(city);
			Thread.Sleep(1050);
			apiHandle.Dispose();
			//
			Assert.AreEqual(1, geocodingRequestCount);
		}
		[TestMethod]
		public void StartPollingCycle_StopPollingAfterDispose()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			int weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForStartPollingCycle(city, null, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city);
			apiHandle.Dispose();
			Thread.Sleep(1050);
			//
			Assert.AreEqual(1, weatherRequestCount);
		}

		private static ApiHandle CreateApiHandleForGetWeatherInCity(byte cachedWeatherLimit,
			Func<string> getCity, Func<long> getWeatherDt,
			IUnixUtcGetter unixUtcGetter, Action? handleGeocodingRequest, Action? handleWeatherRequest)
		{
			const string country = "GB", state = "England";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			const double weatherLat = 51.5074, weatherLon = -0.1278;
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			ApiHandleFactory apiHandleFactory = new ApiHandleFactory(unixUtcGetter,
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				cachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request => {
				switch (request.RequestUri?.LocalPath)
				{
					case geocodingRequestLocalPath:
						handleGeocodingRequest?.Invoke();
						return BuildGeocodingResponse(getCity(), geoLat, geoLon, country, state);
					case weatherRequestLocalPath:
						handleWeatherRequest?.Invoke();
						return BuildWeatherResponse(weatherLat, weatherLon, getWeatherDt(), getCity(), country);
				}
				return null;
			}));
			return apiHandleFactory.Make(httpClient, apiKey, updateMode);
		}

		[TestMethod]
		public void GetWeatherInCity_FirstRequest()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			string weather = apiHandle.GetWeatherInCity(city);
			//
			Assert.IsNotNull(weather);
			Assert.AreEqual(1, geocodingRequestCount);
			Assert.AreEqual(1, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_CachingBelowCacheLimit()
		{
			ApiHandleFactory.DisposeAll();
			const string city1 = "London", city2 = "Karaganda";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			string? city = null;
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(2,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city = city1);
			apiHandle.GetWeatherInCity(city = city2);
			apiHandle.GetWeatherInCity(city = city1);
			//
			Assert.AreEqual(2, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_CachingOverCacheLimit()
		{
			ApiHandleFactory.DisposeAll();
			const string city1 = "London", city2 = "Karaganda";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			string? city = null;
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(1,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city = city1);
			apiHandle.GetWeatherInCity(city = city2);
			apiHandle.GetWeatherInCity(city = city1);
			//
			Assert.AreEqual(3, geocodingRequestCount);
			Assert.AreEqual(3, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_CachingWithZeroLimit()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(0,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(city);
			apiHandle.GetWeatherInCity(city);
			//
			Assert.AreEqual(2, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_GettingCachedWeather()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter();
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			unixUtcGetter.Seconds = weatherDt;
			apiHandle.GetWeatherInCity(city);
			unixUtcGetter.Seconds = weatherDt + ApiHandleFactory.DefaultWeatherRelevancePeriod;
			apiHandle.GetWeatherInCity(city);
			//
			Assert.AreEqual(1, geocodingRequestCount);
			Assert.AreEqual(1, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_UpdatingCachedWeather()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter();
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				() => city, () => weatherDt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			unixUtcGetter.Seconds = weatherDt;
			apiHandle.GetWeatherInCity(city);
			unixUtcGetter.Seconds = weatherDt + ApiHandleFactory.DefaultWeatherRelevancePeriod + 1L;
			apiHandle.GetWeatherInCity(city);
			//
			Assert.AreEqual(1, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_NullCityName_ThrowsArgumentNullException()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				() => city, () => weatherDt, unixUtcGetter, null, null);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(null);
			Assert.ThrowsException<ArgumentNullException>(gwic);
		}
		[TestMethod]
		public void GetWeatherInCity_Disposed_ThrowsInvalidOperationException()
		{
			ApiHandleFactory.DisposeAll();
			const string city = "London";
			const long weatherDt = 1098L;
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = weatherDt };
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				() => city, () => weatherDt, unixUtcGetter, null, null);
			//
			apiHandle.Dispose();
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<InvalidOperationException>(gwic);
		}

		[TestMethod]
		public void RemoveOldestCachedWeather_UserRequestDtIsCriteria()
		{
			ApiHandleFactory.DisposeAll();
			CityName_WeatherDt cw = new CityName_WeatherDt();
			var cws = new[] {
				new CityName_WeatherDt { City = "London", Dt = 995L },
				new CityName_WeatherDt { City = "Karaganda", Dt = 998L },
				new CityName_WeatherDt { City = "Norilsk", Dt = 993L },
			};
			var unixUtcGetter = new FakeUnixUtcGetter();
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(2,
				() => cw.City, () => cw.Dt, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			Action<int, long> makeRequest = (cwi, dtNow) => {
				cws[cwi].AssignTo(cw);
				unixUtcGetter.Seconds = dtNow;
				apiHandle.GetWeatherInCity(cw.City);
			};
			long dtNow = 1000L;
			makeRequest(0, dtNow++);
			makeRequest(1, dtNow++);
			makeRequest(0, dtNow++);
			//
			makeRequest(2, dtNow++);
			//
			makeRequest(0, dtNow++);
			Assert.AreEqual(3, geocodingRequestCount);
			Assert.AreEqual(3, weatherRequestCount);
			makeRequest(1, dtNow++);
			Assert.AreEqual(4, geocodingRequestCount);
			Assert.AreEqual(4, weatherRequestCount);
		}

		// TODO: Write unit tests for 'RequestForWeather' method
		// TODO: Write unit tests for 'RequestForCityLocation' method

		private class CityName_WeatherDt
		{
			public string City { get; set; }
			public long Dt { get; set; }

			public void AssignTo(CityName_WeatherDt oth)
			{
				oth.City = City;
				oth.Dt = Dt;
			}
		}
	}
}
