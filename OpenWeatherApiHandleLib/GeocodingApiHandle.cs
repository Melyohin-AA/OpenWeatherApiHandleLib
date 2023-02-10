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
			string response = GetResponse(request, httpClient);
			return GetCityLocationFromResponse(cityName, response, culture);
		}
		private static string BuildRequest(string cityName, string apiKey, string url)
		{
			System.Collections.Specialized.NameValueCollection parameters =
				System.Web.HttpUtility.ParseQueryString(string.Empty);
			parameters.Add("q", cityName);
			parameters.Add("limit", "1");
			parameters.Add("appid", apiKey);
			return $"{url}?{parameters}";
		}
		private static string GetResponse(string request, HttpClient httpClient)
		{
			Task<HttpResponseMessage> task = httpClient.GetAsync(request);
			task.Wait();
			switch (task.Result.StatusCode)
			{
				case System.Net.HttpStatusCode.OK: break;
				default: throw new Exceptions.UnexpectedStatusCodeException(task.Result.StatusCode, request);
			}
			var responseReadTask = task.Result.Content.ReadAsStringAsync();
			responseReadTask.Wait();
			return responseReadTask.Result;
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

		public class CityLocation
		{
			public string name;
			public Dictionary<string, string> local_names;
			public double lat, lon;
			public string country;
		}
	}
}
