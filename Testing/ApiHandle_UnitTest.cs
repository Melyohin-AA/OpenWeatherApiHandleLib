using System;
using System.Globalization;

namespace OpenWeatherApiHandleLib.Testing
{
	[TestClass]
	public class ApiHandle_UnitTest
	{
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

		// TODO: Write unit tests for ApiHandle methods
	}
}
