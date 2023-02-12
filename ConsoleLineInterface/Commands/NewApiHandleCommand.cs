using System;
using System.Text.RegularExpressions;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class NewApiHandleCommand : BaseCommand
	{
		private static readonly Regex regex = new Regex(@"new-hdl \w+ [a-z\d]+ [dp]");

		public override string Description => "Create new ApiHandle object";
		public override string Tip => "new-hdl <Name> <ApiKey> <UpdateMode[d:OnDemand|p:Polling]>";

		public override bool IsMatch(string cmd) => regex.IsMatch(cmd);

		public override string Execute(Context context, string cmd)
		{
			string[] args = cmd.Split();
			string name = args[1];
			string apiKey = args[2];
			var updateMode = (args[3] == "d") ? ApiHandleUpdateMode.OnDemand : ApiHandleUpdateMode.Polling;
			if (context.ApiHandles.ContainsKey(name))
				return "Specified name is occupied";
			try
			{
				ApiHandle apiHandle = context.ApiHandleFactory.Make(apiKey, updateMode);
				context.ApiHandles.Add(name, apiHandle);
				if (updateMode == ApiHandleUpdateMode.Polling)
				{
					ApiHandle.ExceptionHandler exceptionHandler = Program.MakePollingLoopExceptionHandler(name);
					apiHandle.PollingLoopExceptionEvent += exceptionHandler;
					context.PollingLoopExceptionHandlers.Add(name, exceptionHandler);
				}
			}
			catch (Exceptions.ApiKeyOccupiedException ex)
			{
				return ex.Message;
			}
			return "Success";
		}
	}
}
