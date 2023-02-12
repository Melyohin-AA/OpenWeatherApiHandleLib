using System;
using System.Text;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface.Commands
{
	internal class HelpCommand : BaseCommand
	{
		public override string Description => "Print list of all commands";
		public override string Tip => "help";

		public override bool IsMatch(string cmd) => cmd == "help";

		public override string Execute(Context context, string cmd)
		{
			var sb = new StringBuilder("\tCommand list:\n");
			foreach (BaseCommand command in context.CommandList)
			{
				sb.Append("  ");
				sb.Append(command.Description);
				sb.Append(":\n> ");
				sb.Append(command.Tip);
				sb.Append("\n");
			}
			return sb.ToString();
		}
	}
}
