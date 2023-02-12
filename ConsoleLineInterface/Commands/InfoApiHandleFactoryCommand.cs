using System;
using System.Text;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class InfoApiHandleFactoryCommand : BaseCommand
	{
		public override string Description => "Print info about the ApiHandleFactory object";
		public override string Tip => "info-factory";

		public override bool IsMatch(string cmd) => cmd == "info-factory";

		public override string Execute(Context context, string cmd)
		{
			var sb = new StringBuilder($"\tApiHandleFactory:\n");
			sb.Append($"Current UNIX UTC DT    = {context.ApiHandleFactory.UnixUtcGetter.Seconds} s\n");
			sb.Append($"GeocodingApiUrl        = {context.ApiHandleFactory.GeocodingApiUrl}\n");
			sb.Append($"WeatherApiUrl          = {context.ApiHandleFactory.WeatherApiUrl}\n");
			sb.Append($"CachedWeatherLimit     = {context.ApiHandleFactory.CachedWeatherLimit}\n");
			sb.Append($"WeatherRelevancePeriod = {context.ApiHandleFactory.WeatherRelevancePeriod} s\n");
			return sb.ToString();
		}
	}
}
