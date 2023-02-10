using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OpenWeatherApiHandleLib
{
	internal static class WeatherApiHandle
	{
		public static Weather GetWeatherAtLocation(WeatherCoord location, string apiKey, string url,
			HttpClient httpClient, CultureInfo culture)
		{
			string request = BuildRequest(location, apiKey, url, culture);
			string response = GetResponse(request, httpClient, apiKey);
			return GetWeatherFromResponse(response, culture);
		}
		private static string BuildRequest(WeatherCoord location, string apiKey, string url, CultureInfo culture)
		{
			System.Collections.Specialized.NameValueCollection parameters =
				System.Web.HttpUtility.ParseQueryString(string.Empty);
			parameters.Add("lat", location.lat.ToString(culture));
			parameters.Add("lon", location.lon.ToString(culture));
			parameters.Add("appid", apiKey);
			return $"{url}?{parameters}";
		}
		private static string GetResponse(string request, HttpClient httpClient, string apiKey)
		{
			Task<HttpResponseMessage> task = httpClient.GetAsync(request);
			task.Wait();
			switch (task.Result.StatusCode)
			{
				case System.Net.HttpStatusCode.OK:
					break;
				case System.Net.HttpStatusCode.Unauthorized:
					throw new Exceptions.InvalidApiKeyException($"The API considers '{apiKey}' key as invalid one!");
				default:
					throw new Exceptions.UnexpectedStatusCodeException(task.Result.StatusCode, request);
			}
			var responseReadTask = task.Result.Content.ReadAsStringAsync();
			responseReadTask.Wait();
			return responseReadTask.Result;
		}
		private static Weather GetWeatherFromResponse(string response, CultureInfo culture)
		{
			var serSettings = new JsonSerializerSettings { Culture = culture };
			var weather = JsonConvert.DeserializeObject<Weather>(response, serSettings);
			return weather;
		}

		public class Weather
		{
			public WeatherCoord coord;
			public WeatherCondition[] weather;
			public WeatherMain main;
			public ushort visibility;
			public WeatherWind wind;
			public long dt;
			public WeatherSys sys;
			public int timezone;
			public string name;

			public string ToJson(CultureInfo culture)
			{
				var serSettings = new JsonSerializerSettings { Culture = culture };
				return JsonConvert.SerializeObject(this, serSettings);
			}
		}

		public struct WeatherCoord
		{
			public double lat, lon;
		}

		public struct WeatherCondition
		{
			public ushort id;
			public string main;
			public string description;
		}

		public struct WeatherMain
		{
			public double temp, feels_like;
			public ushort pressure;
			public byte humidity;
		}

		public struct WeatherWind
		{
			public double speed;
			public short deg;
		}

		public struct WeatherSys
		{
			public long sunrise;
			public long sunset;
		}
	}
}
