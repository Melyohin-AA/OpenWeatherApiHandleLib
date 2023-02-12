using System;
using System.Text.RegularExpressions;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class DeleteApiHandleCommand : BaseCommand
	{
		private static readonly Regex regex = new Regex(@"del-hdl \w+");

		public override string Description => "Delete an ApiHandle object";
		public override string Tip => "del-hdl <ApiHandleName>";

		public override bool IsMatch(string cmd) => regex.IsMatch(cmd);

		public override string Execute(Context context, string cmd)
		{
			string apiHandleName = cmd.Substring(cmd.IndexOf(' ') + 1);
			bool apiHandleFound = context.ApiHandles.TryGetValue(apiHandleName, out ApiHandle apiHandle);
			if (!apiHandleFound)
				return apiHandleNotFoundMessage;
			if (apiHandle.UpdateMode == ApiHandleUpdateMode.Polling)
			{
				ApiHandle.ExceptionHandler exceptionHandler = context.PollingLoopExceptionHandlers[apiHandleName];
				apiHandle.PollingLoopExceptionEvent -= exceptionHandler;
				context.PollingLoopExceptionHandlers.Remove(apiHandleName);
			}
			apiHandle.Dispose();
			context.ApiHandles.Remove(apiHandleName);
			return "Success";
		}
	}
}
