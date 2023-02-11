using System;
using System.Net;
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

		private static FakeHttpMessageHandler.ResponseFunc CreateRegularResponseFunc(CityName_WeatherDt cw,
			double weatherLat, double weatherLon, double geoLat, double geoLon,
			Action? handleGeocodingRequest, Action? handleWeatherRequest)
		{
			const string country = "GB", state = "England";
			return request => {
				switch (request.RequestUri?.LocalPath)
				{
					case geocodingRequestLocalPath:
						handleGeocodingRequest?.Invoke();
						return (BuildGeocodingResponse(cw.City, geoLat, geoLon, country, state), HttpStatusCode.OK);
					case weatherRequestLocalPath:
						handleWeatherRequest?.Invoke();
						return (BuildWeatherResponse(weatherLat, weatherLon, cw.Dt, cw.City, country),
							HttpStatusCode.OK);
				}
				return (null, HttpStatusCode.NotFound);
			};
		}

		private static ApiHandle CreateApiHandle(string apiKey, ApiHandleUpdateMode updateMode,
			byte cachedWeatherLimit, long weatherRelevancePeriod,
			IUnixUtcGetter unixUtcGetter, FakeHttpMessageHandler.ResponseFunc responseFunc)
		{
			ApiHandleFactory apiHandleFactory = new ApiHandleFactory(unixUtcGetter,
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				cachedWeatherLimit, weatherRelevancePeriod);
			var httpClient = new HttpClient(new FakeHttpMessageHandler(responseFunc));
			return apiHandleFactory.Make(httpClient, apiKey, updateMode);
		}

		private static ApiHandle CreateApiHandleForStartPollingCycle(string city,
			Action? handleGeocodingRequest, Action? handleWeatherRequest)
		{
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			const double weatherLat = 51.5074, weatherLon = -0.1278;
			const string apiKey = "abc123";
			var unixUtcGetter = new SystemUnixUtcGetter();
			const long weatherRelevancePeriod = 1L;
			FakeHttpMessageHandler.ResponseFunc responseFunc = CreateRegularResponseFunc(
				new CityName_WeatherDt { City = city, Dt = unixUtcGetter.Seconds },
				weatherLat, weatherLon, geoLat, geoLon, handleGeocodingRequest, handleWeatherRequest);
			return CreateApiHandle(apiKey, ApiHandleUpdateMode.Polling,
				ApiHandleFactory.DefaultCachedWeatherLimit, weatherRelevancePeriod,
				unixUtcGetter, responseFunc);
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
			Thread.Sleep(1200);
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
			CityName_WeatherDt cw, IUnixUtcGetter unixUtcGetter,
			Action? handleGeocodingRequest, Action? handleWeatherRequest)
		{
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			const double weatherLat = 51.5074, weatherLon = -0.1278;
			const string apiKey = "abc123";
			FakeHttpMessageHandler.ResponseFunc responseFunc = CreateRegularResponseFunc(cw,
				weatherLat, weatherLon, geoLat, geoLon, handleGeocodingRequest, handleWeatherRequest);
			return CreateApiHandle(apiKey, ApiHandleUpdateMode.OnDemand,
				cachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod,
				unixUtcGetter, responseFunc);
		}

		[TestMethod]
		public void GetWeatherInCity_FirstRequest()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				cw, unixUtcGetter, () => geocodingRequestCount++, () => weatherRequestCount++);
			//
			string weather = apiHandle.GetWeatherInCity(cw.City);
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
			var cw = new CityName_WeatherDt { Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(2, cw, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(cw.City = city1);
			apiHandle.GetWeatherInCity(cw.City = city2);
			apiHandle.GetWeatherInCity(cw.City = city1);
			//
			Assert.AreEqual(2, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_CachingOverCacheLimit()
		{
			ApiHandleFactory.DisposeAll();
			const string city1 = "London", city2 = "Karaganda";
			var cw = new CityName_WeatherDt { Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(1, cw, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(cw.City = city1);
			apiHandle.GetWeatherInCity(cw.City = city2);
			apiHandle.GetWeatherInCity(cw.City = city1);
			//
			Assert.AreEqual(3, geocodingRequestCount);
			Assert.AreEqual(3, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_CachingWithZeroLimit()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(0, cw, unixUtcGetter,
				() => geocodingRequestCount++, () => weatherRequestCount++);
			//
			apiHandle.GetWeatherInCity(cw.City);
			apiHandle.GetWeatherInCity(cw.City);
			//
			Assert.AreEqual(2, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_GettingCachedWeather()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter();
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				cw, unixUtcGetter, () => geocodingRequestCount++, () => weatherRequestCount++);
			//
			unixUtcGetter.Seconds = cw.Dt;
			apiHandle.GetWeatherInCity(cw.City);
			unixUtcGetter.Seconds = cw.Dt + ApiHandleFactory.DefaultWeatherRelevancePeriod;
			apiHandle.GetWeatherInCity(cw.City);
			//
			Assert.AreEqual(1, geocodingRequestCount);
			Assert.AreEqual(1, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_UpdatingCachedWeather()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter();
			int geocodingRequestCount = 0, weatherRequestCount = 0;
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				cw, unixUtcGetter, () => geocodingRequestCount++, () => weatherRequestCount++);
			//
			unixUtcGetter.Seconds = cw.Dt;
			apiHandle.GetWeatherInCity(cw.City);
			unixUtcGetter.Seconds = cw.Dt + ApiHandleFactory.DefaultWeatherRelevancePeriod + 1L;
			apiHandle.GetWeatherInCity(cw.City);
			//
			Assert.AreEqual(1, geocodingRequestCount);
			Assert.AreEqual(2, weatherRequestCount);
		}
		[TestMethod]
		public void GetWeatherInCity_NullCityName_ThrowsArgumentNullException()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				cw, unixUtcGetter, null, null);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(null);
			Assert.ThrowsException<ArgumentNullException>(gwic);
		}
		[TestMethod]
		public void GetWeatherInCity_Disposed_ThrowsInvalidOperationException()
		{
			ApiHandleFactory.DisposeAll();
			var cw = new CityName_WeatherDt { City = "London", Dt = 1098L };
			var unixUtcGetter = new FakeUnixUtcGetter { Seconds = cw.Dt };
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(ApiHandleFactory.DefaultCachedWeatherLimit,
				cw, unixUtcGetter, null, null);
			//
			apiHandle.Dispose();
			Action gwic = () => apiHandle.GetWeatherInCity(cw.City);
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
			ApiHandle apiHandle = CreateApiHandleForGetWeatherInCity(2, cw, unixUtcGetter,
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

		private static ApiHandle CreateApiHandleForRequests(string apiKey, string city, double geoLat, double geoLon,
			Action<Uri>? pushGeocodingRequestUri, Action<Uri>? pushWeatherRequestUri, out string expectedWeather)
		{
			const string country = "GB", state = "England";
			const double weatherLat = 51.5074, weatherLon = -0.1278;
			const long dt = 1675945807L;
			string geocodingResponse = BuildGeocodingResponse(city, geoLat, geoLon, country, state);
			string weatherResponse = BuildWeatherResponse(weatherLat, weatherLon, dt, city, country);
			expectedWeather = BuildWeatherJson(weatherLat, weatherLon, dt, city);
			FakeHttpMessageHandler.ResponseFunc responseFunc = request => {
				switch (request.RequestUri?.LocalPath)
				{
					case geocodingRequestLocalPath:
						pushGeocodingRequestUri?.Invoke(request.RequestUri);
						return (geocodingResponse, HttpStatusCode.OK);
					case weatherRequestLocalPath:
						pushWeatherRequestUri?.Invoke(request.RequestUri);
						return (weatherResponse, HttpStatusCode.OK);
				}
				return (null, HttpStatusCode.NotFound);
			};
			return CreateApiHandle(apiKey, ApiHandleUpdateMode.OnDemand,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod,
				new SystemUnixUtcGetter(), responseFunc);
		}

		[TestMethod]
		public void RequestForWeatherByCityName_Result()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "London";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			Uri? geocodingRequestUri = null, weatherRequestUri = null;
			ApiHandle apiHandle = CreateApiHandleForRequests(apiKey, city, geoLat, geoLon,
				uri => geocodingRequestUri = uri, uri => weatherRequestUri = uri, out string expectedWeather);
			//
			string actualWeather = apiHandle.GetWeatherInCity(city);
			//
			Assert.AreEqual(expectedWeather, actualWeather);
		}

		[TestMethod]
		public void RequestForWeatherByCityLocation_RequestUri()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "London";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			Uri? geocodingRequestUri = null, weatherRequestUri = null;
			ApiHandle apiHandle = CreateApiHandleForRequests(apiKey, city, geoLat, geoLon,
				uri => geocodingRequestUri = uri, uri => weatherRequestUri = uri, out string expectedWeather);
			//
			string actualWeather = apiHandle.GetWeatherInCity(city);
			//
			var geocodingQuery = System.Web.HttpUtility.ParseQueryString(geocodingRequestUri.Query);
			Assert.AreEqual(city, geocodingQuery["q"]);
			Assert.AreEqual(1, int.Parse(geocodingQuery["limit"]));
			Assert.AreEqual(apiKey, geocodingQuery["appid"]);
		}
		[TestMethod]
		public void RequestForWeatherByCityLocation_404StatusCode_ThrowsUnexpectedStatusCodeException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "Abyrvalg", country = "GB", state = "England";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			string geocodingResponse = BuildGeocodingResponse(city, geoLat, geoLon, country, state);
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
				(request.RequestUri?.LocalPath == geocodingRequestLocalPath) ?
				(geocodingResponse, HttpStatusCode.OK) : (null, HttpStatusCode.NotFound)));
			var factory = new ApiHandleFactory(new SystemUnixUtcGetter(),
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			ApiHandle apiHandle = factory.Make(httpClient, apiKey, ApiHandleUpdateMode.OnDemand);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<Exceptions.UnexpectedStatusCodeException>(gwic);
		}
		[TestMethod]
		public void RequestForWeatherByCityLocation_401StatusCode_ThrowsInvalidApiKeyException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "Abyrvalg", country = "GB", state = "England";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			string geocodingResponse = BuildGeocodingResponse(city, geoLat, geoLon, country, state);
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
				(request.RequestUri?.LocalPath == geocodingRequestLocalPath) ?
				(geocodingResponse, HttpStatusCode.OK) : (null, HttpStatusCode.Unauthorized)));
			var factory = new ApiHandleFactory(new SystemUnixUtcGetter(),
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			ApiHandle apiHandle = factory.Make(httpClient, apiKey, ApiHandleUpdateMode.OnDemand);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<Exceptions.InvalidApiKeyException>(gwic);
		}

		[TestMethod]
		public void RequestForCityLocation_RequestUri()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "London";
			const double geoLat = 51.5073219, geoLon = -0.1276474;
			Uri? geocodingRequestUri = null, weatherRequestUri = null;
			ApiHandle apiHandle = CreateApiHandleForRequests(apiKey, city, geoLat, geoLon,
				uri => geocodingRequestUri = uri, uri => weatherRequestUri = uri, out string expectedWeather);
			//
			string actualWeather = apiHandle.GetWeatherInCity(city);
			//
			var weatherQuery = System.Web.HttpUtility.ParseQueryString(weatherRequestUri.Query);
			Assert.AreEqual(geoLat, double.Parse(weatherQuery["lat"]));
			Assert.AreEqual(geoLon, double.Parse(weatherQuery["lon"]));
			Assert.AreEqual(apiKey, weatherQuery["appid"]);
		}
		[TestMethod]
		public void RequestForCityLocation_LocationNotFound_ThrowsLocationNotFoundException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "Abyrvalg";
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request => ("[]", HttpStatusCode.OK)));
			var factory = new ApiHandleFactory(new SystemUnixUtcGetter(),
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			ApiHandle apiHandle = factory.Make(httpClient, apiKey, ApiHandleUpdateMode.OnDemand);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<Exceptions.LocationNotFoundException>(gwic);
		}
		[TestMethod]
		public void RequestForCityLocation_404StatusCode_ThrowsUnexpectedStatusCodeException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "Abyrvalg";
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request => (null, HttpStatusCode.NotFound)));
			var factory = new ApiHandleFactory(new SystemUnixUtcGetter(),
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			ApiHandle apiHandle = factory.Make(httpClient, apiKey, ApiHandleUpdateMode.OnDemand);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<Exceptions.UnexpectedStatusCodeException>(gwic);
		}
		[TestMethod]
		public void RequestForCityLocation_401StatusCode_ThrowsInvalidApiKeyException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			const string city = "Abyrvalg";
			var httpClient = new HttpClient(new FakeHttpMessageHandler(request => (null, HttpStatusCode.Unauthorized)));
			var factory = new ApiHandleFactory(new SystemUnixUtcGetter(),
				ApiHandleFactory.DefaultGeocodingApiUrl, ApiHandleFactory.DefaultWeatherApiUrl,
				ApiHandleFactory.DefaultCachedWeatherLimit, ApiHandleFactory.DefaultWeatherRelevancePeriod);
			ApiHandle apiHandle = factory.Make(httpClient, apiKey, ApiHandleUpdateMode.OnDemand);
			//
			Action gwic = () => apiHandle.GetWeatherInCity(city);
			Assert.ThrowsException<Exceptions.InvalidApiKeyException>(gwic);
		}

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
