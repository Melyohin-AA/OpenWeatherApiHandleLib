using System;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class DeleteAllApiHandlesCommand : BaseCommand
	{
		public override string Description => "Delete all ApiHandle objects";
		public override string Tip => "del-all-hdl";

		public override bool IsMatch(string cmd) => cmd == "del-all-hdl";

		public override string Execute(Context context, string cmd)
		{
			ApiHandleFactory.DisposeAll();
			context.ApiHandles.Clear();
			context.PollingLoopExceptionHandlers.Clear();
			return "Success";
		}
	}
}
