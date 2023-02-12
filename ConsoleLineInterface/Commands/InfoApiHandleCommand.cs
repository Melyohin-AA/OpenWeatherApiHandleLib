using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class InfoApiHandleCommand : BaseCommand
	{
		private static readonly Regex regex = new Regex(@"info-hdl \w+");

		public override string Description => "Print info about an ApiHandle object";
		public override string Tip => "info-hdl <ApiHandleName>";

		public override bool IsMatch(string cmd) => regex.IsMatch(cmd);

		public override string Execute(Context context, string cmd)
		{
			string apiHandleName = cmd.Substring(cmd.IndexOf(' ') + 1);
			bool apiHandleFound = context.ApiHandles.TryGetValue(apiHandleName, out ApiHandle apiHandle);
			if (!apiHandleFound)
				return apiHandleNotFoundMessage;
			var sb = new StringBuilder($"\t{apiHandleName}:\n");
			sb.Append($"ApiKey     = {apiHandle.ApiKey}\n");
			sb.Append($"UpdateMode = {apiHandle.UpdateMode}\n");
			return sb.ToString();
		}
	}
}
