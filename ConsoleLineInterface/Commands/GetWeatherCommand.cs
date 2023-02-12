using System;
using System.Text.RegularExpressions;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class GetWeatherCommand : BaseCommand
	{
		private static readonly Regex regex = new Regex(@"get-weather \w+ \w+");

		public override string Description => "Get weather data at a specific city location in JSON format";
		public override string Tip => "get-weather <ApiHandleName> <CityName>";

		public override bool IsMatch(string cmd) => regex.IsMatch(cmd);

		public override string Execute(Context context, string cmd)
		{
			string[] args = cmd.Split();
			string apiHandleName = args[1];
			string cityName = args[2];
			bool apiHandleFound = context.ApiHandles.TryGetValue(apiHandleName, out ApiHandle apiHandle);
			if (!apiHandleFound)
				return apiHandleNotFoundMessage;
			try
			{
				return apiHandle.GetWeatherInCity(cityName);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
