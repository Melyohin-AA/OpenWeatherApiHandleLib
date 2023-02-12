using System;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class ExitCommand : BaseCommand
	{
		public override string Description => "Exit the program";
		public override string Tip => "exit";

		public override bool IsMatch(string cmd) => cmd == "exit";

		public override string Execute(Context context, string cmd)
		{
			context.ExitFlag = true;
			return "Exiting the program . . . ";
		}
	}
}
