using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Represents the interface for geocoding direct calls.
	/// </summary>
	internal static class GeocodingCaller
	{
		/// <summary>
		/// Searches for the location with the specified city name.
		/// </summary>
		/// <param name="cityName"></param>
		/// <param name="apiKey"></param>
		/// <param name="url"></param>
		/// <param name="httpClient"></param>
		/// <param name="culture"></param>
		/// <returns>The geographical coordinates of the location with the specified city name</returns>
		public static WeatherCaller.WeatherCoord GetCityLocation(string cityName, string apiKey, string url,
			HttpClient httpClient, CultureInfo culture)
		{
			string requestUri = BuildRequestUri(cityName, apiKey, url);
			string response = ApiCallerCommons.GetResponse(requestUri, httpClient, apiKey);
			return GetCityLocationFromResponse(cityName, response, culture);
		}

		/// <summary>
		/// Builds the request URI.
		/// </summary>
		/// <param name="cityName"></param>
		/// <param name="apiKey"></param>
		/// <param name="url"></param>
		/// <returns>Built request URI</returns>
		private static string BuildRequestUri(string cityName, string apiKey, string url)
		{
			return ApiCallerCommons.BuildRequestUri(url, new Dictionary<string, string>() {
				{ "q", cityName },
				{ "limit", "1" },
				{ "appid", apiKey },
			});
		}

		/// <summary>
		/// Gets city location from response JSON.
		/// </summary>
		/// <param name="cityName"></param>
		/// <param name="response"></param>
		/// <param name="culture"></param>
		/// <returns>The geographical coordinates of the city location</returns>
		/// <exception cref="NullReferenceException">If fails to parse JSON</exception>
		/// <exception cref="Exceptions.LocationNotFoundException">If the city location is not found</exception>
		private static WeatherCaller.WeatherCoord GetCityLocationFromResponse(string cityName, string response,
			CultureInfo culture)
		{
			var serSettings = new JsonSerializerSettings { Culture = culture };
			var cities = JsonConvert.DeserializeObject<CityLocation[]>(response, serSettings) ??
				throw new NullReferenceException("Failed to parse response JSON!");
			if (cities.Length == 0) throw new Exceptions.LocationNotFoundException($"'{cityName}' city is not found!");
			CityLocation city = cities[0];
			return new WeatherCaller.WeatherCoord { lat = city.lat, lon = city.lon };
		}

		/// <summary>
		/// Represents response data of geocoding direct call.
		/// </summary>
		public struct CityLocation
		{
			public string name;
			public double lat, lon;
			public string country;
		}
	}
}
