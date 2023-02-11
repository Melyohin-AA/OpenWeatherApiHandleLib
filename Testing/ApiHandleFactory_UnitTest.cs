using System;

namespace OpenWeatherApiHandleLib.Testing
{
	[TestClass]
	public class ApiHandleFactory_UnitTest
	{
		[TestMethod]
		public void DefaultConstructor_InitializesPropertiesWithDefaultValues()
		{
			var apiHandleFactory = new ApiHandleFactory();
			//
			Assert.IsTrue(apiHandleFactory.UnixUtcGetter is SystemUnixUtcGetter);
			Assert.AreEqual(ApiHandleFactory.DefaultGeocodingApiUrl, apiHandleFactory.GeocodingApiUrl);
			Assert.AreEqual(ApiHandleFactory.DefaultWeatherApiUrl, apiHandleFactory.WeatherApiUrl);
			Assert.AreEqual(ApiHandleFactory.DefaultCachedWeatherLimit, apiHandleFactory.CachedWeatherLimit);
			Assert.AreEqual(ApiHandleFactory.DefaultWeatherRelevancePeriod, apiHandleFactory.WeatherRelevancePeriod);
		}

		[TestMethod]
		public void ParametrizedConstructor_InitializesPropertiesWithSpecifiedValues()
		{
			var unixUtcGetter = new SystemUnixUtcGetter();
			const string geocodingApiUrl = "http://geocoding", weatherApiUrl = "http://weather";
			const byte cachedWeatherLimit = 5;
			const long weatherRelevancePeriod = 300L;
			//
			var apiHandleFactory = new ApiHandleFactory(unixUtcGetter, geocodingApiUrl, weatherApiUrl,
				cachedWeatherLimit, weatherRelevancePeriod);
			//
			Assert.AreSame(unixUtcGetter, apiHandleFactory.UnixUtcGetter);
			Assert.AreEqual(geocodingApiUrl, apiHandleFactory.GeocodingApiUrl);
			Assert.AreEqual(weatherApiUrl, apiHandleFactory.WeatherApiUrl);
			Assert.AreEqual(cachedWeatherLimit, apiHandleFactory.CachedWeatherLimit);
			Assert.AreEqual(weatherRelevancePeriod, apiHandleFactory.WeatherRelevancePeriod);
		}
		[TestMethod]
		public void ParametrizedConstructor_NullUnixUtcGetter_ThrowsArgumentNullException()
		{
			const string geocodingApiUrl = "http://geocoding", weatherApiUrl = "http://weather";
			const byte cachedWeatherLimit = 5;
			const long weatherRelevancePeriod = 300L;
			//
			Action c = () => new ApiHandleFactory(null, geocodingApiUrl, weatherApiUrl,
				cachedWeatherLimit, weatherRelevancePeriod);
			Assert.ThrowsException<ArgumentNullException>(c);
		}
		[TestMethod]
		public void ParametrizedConstructor_NullGeocodingApiUrl_ThrowsArgumentNullException()
		{
			var unixUtcGetter = new SystemUnixUtcGetter();
			const string weatherApiUrl = "http://weather";
			const byte cachedWeatherLimit = 5;
			const long weatherRelevancePeriod = 300L;
			//
			Action c = () => new ApiHandleFactory(unixUtcGetter, null, weatherApiUrl,
				cachedWeatherLimit, weatherRelevancePeriod);
			Assert.ThrowsException<ArgumentNullException>(c);
		}
		[TestMethod]
		public void ParametrizedConstructor_NullWeatherApiUrl_ThrowsArgumentNullException()
		{
			var unixUtcGetter = new SystemUnixUtcGetter();
			const string geocodingApiUrl = "http://geocoding";
			const byte cachedWeatherLimit = 5;
			const long weatherRelevancePeriod = 300L;
			//
			Action c = () => new ApiHandleFactory(unixUtcGetter, geocodingApiUrl, null,
				cachedWeatherLimit, weatherRelevancePeriod);
			Assert.ThrowsException<ArgumentNullException>(c);
		}
		[TestMethod]
		public void ParametrizedConstructor_NegativeWeatherRelevancePeriod_ThrowsArgumentOutOfRangeException()
		{
			var unixUtcGetter = new SystemUnixUtcGetter();
			const string geocodingApiUrl = "http://geocoding", weatherApiUrl = "http://weather";
			const byte cachedWeatherLimit = 5;
			const long weatherRelevancePeriod = -1L;
			//
			Action c = () => new ApiHandleFactory(unixUtcGetter, geocodingApiUrl, weatherApiUrl,
				cachedWeatherLimit, weatherRelevancePeriod);
			Assert.ThrowsException<ArgumentOutOfRangeException>(c);
		}

		private static HttpClient MakeFakeHttpClient()
		{
			return new HttpClient(new FakeHttpMessageHandler(request => {
				Assert.Fail();
				return null;
			}));
		}

		[TestMethod]
		public void Make_Valid()
		{
			ApiHandleFactory.DisposeAll();
			var httpClient = MakeFakeHttpClient();
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			//
			ApiHandle apiHandle = apiHandleFactory.Make(httpClient, apiKey, updateMode);
			//
			Assert.AreSame(httpClient, apiHandle.HttpClient);
			Assert.AreEqual(apiKey, apiHandle.ApiKey);
			Assert.AreEqual(updateMode, apiHandle.UpdateMode);
			Assert.AreSame(apiHandleFactory, apiHandle.Factory);
		}
		[TestMethod]
		public void Make_NullHttpClient_ArgumentNullException()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			//
			Action m = () => apiHandleFactory.Make(null, apiKey, updateMode);
			Assert.ThrowsException<ArgumentNullException>(m);
		}
		[TestMethod]
		public void Make_NullApiKey_ArgumentNullException()
		{
			ApiHandleFactory.DisposeAll();
			var httpClient = MakeFakeHttpClient();
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			//
			Action m = () => apiHandleFactory.Make(httpClient, null, updateMode);
			Assert.ThrowsException<ArgumentNullException>(m);
		}
		[TestMethod]
		public void Make_ApiKeyUsedTwice_ApiKeyOccupiedException()
		{
			ApiHandleFactory.DisposeAll();
			var httpClient = MakeFakeHttpClient();
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			ApiHandle apiHandle1 = apiHandleFactory.Make(httpClient, apiKey, updateMode);
			//
			Action m = () => apiHandleFactory.Make(httpClient, apiKey, updateMode);
			Assert.ThrowsException<Exceptions.ApiKeyOccupiedException>(m);
		}

		[TestMethod]
		public void DisposeAll()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			ApiHandle apiHandle = apiHandleFactory.Make(apiKey, updateMode);
			//
			ApiHandleFactory.DisposeAll();
			//
			Assert.IsTrue(apiHandle.Disposed);
		}

		[TestMethod]
		public void DisposeOne()
		{
			ApiHandleFactory.DisposeAll();
			const string apiKey = "abc123";
			var updateMode = ApiHandleUpdateMode.OnDemand;
			var apiHandleFactory = new ApiHandleFactory();
			ApiHandle apiHandle1 = apiHandleFactory.Make(apiKey, updateMode);
			apiHandle1.Dispose();
			//
			ApiHandle apiHandle2 = apiHandleFactory.Make(apiKey, updateMode);
			//
			Assert.IsFalse(apiHandle2.Disposed);
		}
	}
}
