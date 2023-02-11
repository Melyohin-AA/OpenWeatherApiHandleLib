using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Represents the interface for current weather data calls.
	/// </summary>
	internal static class WeatherCaller
	{
		/// <summary>
		/// Retrieves current weather data for the specified location.
		/// </summary>
		/// <param name="location"></param>
		/// <param name="apiKey"></param>
		/// <param name="url"></param>
		/// <param name="httpClient"></param>
		/// <param name="culture"></param>
		/// <returns>The weather at the specified location</returns>
		public static Weather GetWeatherAtLocation(WeatherCoord location, string apiKey, string url,
			HttpClient httpClient, CultureInfo culture)
		{
			string requestUri = BuildRequestUri(location, apiKey, url, culture);
			string response = ApiCallerCommons.GetResponse(requestUri, httpClient, apiKey);
			return GetWeatherFromResponse(response, culture);
		}

		/// <summary>
		/// Builds the request URI.
		/// </summary>
		/// <param name="location"></param>
		/// <param name="apiKey"></param>
		/// <param name="url"></param>
		/// <param name="culture"></param>
		/// <returns>Built request URI</returns>
		private static string BuildRequestUri(WeatherCoord location, string apiKey, string url, CultureInfo culture)
		{
			return ApiCallerCommons.BuildRequestUri(url, new Dictionary<string, string>() {
				{ "lat", location.lat.ToString(culture) },
				{ "lon", location.lon.ToString(culture) },
				{ "appid", apiKey },
			});
		}

		/// <summary>
		/// Gets weather data from response JSON.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="culture"></param>
		/// <returns>The weather at the location</returns>
		private static Weather GetWeatherFromResponse(string response, CultureInfo culture)
		{
			var serSettings = new JsonSerializerSettings { Culture = culture };
			var weather = JsonConvert.DeserializeObject<Weather>(response, serSettings);
			return weather;
		}

		/// <summary>
		/// Represents response data of current weather data call.
		/// </summary>
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

			/// <summary>
			/// Converts the weather data to JSON.
			/// </summary>
			/// <param name="culture"></param>
			/// <returns>JSON with the weather data</returns>
			public string ToJson(CultureInfo culture)
			{
				var serSettings = new JsonSerializerSettings { Culture = culture };
				return JsonConvert.SerializeObject(this, serSettings);
			}
		}

		/// <summary>
		/// Represents geographical coordinates of a 'Weather' object.
		/// </summary>
		public struct WeatherCoord
		{
			public double lat, lon;
		}

		/// <summary>
		/// Represents weather condition of a 'Weather' object.
		/// </summary>
		public struct WeatherCondition
		{
			public ushort id;
			public string main;
			public string description;
		}

		/// <summary>
		/// Represents main values of a 'Weather' object.
		/// </summary>
		public struct WeatherMain
		{
			public double temp, feels_like;
			public ushort pressure;
			public byte humidity;
		}

		/// <summary>
		/// Represents wind values of a 'Weather' object.
		/// </summary>
		public struct WeatherWind
		{
			public double speed;
			public short deg;
		}

		/// <summary>
		/// Represents sunrise and sunset datetimes of a 'Weather' object.
		/// </summary>
		public struct WeatherSys
		{
			public long sunrise;
			public long sunset;
		}
	}
}
