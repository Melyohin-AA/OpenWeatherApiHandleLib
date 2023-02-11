using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OpenWeatherApiHandleLib
{
	internal static class GeocodingApiHandle
	{
		public static WeatherApiHandle.WeatherCoord GetCityLocation(string cityName, string apiKey, string url,
			HttpClient httpClient, CultureInfo culture)
		{
			string request = BuildRequest(cityName, apiKey, url);
			string response = ApiHandleCommons.GetResponse(request, httpClient, apiKey);
			return GetCityLocationFromResponse(cityName, response, culture);
		}
		private static string BuildRequest(string cityName, string apiKey, string url)
		{
			return ApiHandleCommons.BuildRequest(url, new Dictionary<string, string>() {
				{ "q", cityName },
				{ "limit", "1" },
				{ "appid", apiKey },
			});
		}
		private static WeatherApiHandle.WeatherCoord GetCityLocationFromResponse(string cityName, string response,
			CultureInfo culture)
		{
			var serSettings = new JsonSerializerSettings { Culture = culture };
			var cities = JsonConvert.DeserializeObject<CityLocation[]>(response, serSettings) ??
				throw new NullReferenceException();
			if (cities.Length == 0) throw new Exceptions.LocationNotFoundException($"'{cityName}' city is not found!");
			CityLocation city = cities[0];
			return new WeatherApiHandle.WeatherCoord { lat = city.lat, lon = city.lon };
		}

		public struct CityLocation
		{
			public string name;
			public double lat, lon;
			public string country;
		}
	}
}
