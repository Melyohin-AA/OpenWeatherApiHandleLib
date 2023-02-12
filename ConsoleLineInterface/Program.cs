using System;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface
{
	static class Program
	{
		private static Context context;

		static void Main(string[] args)
		{
			context = new Context(InitCommandList());
			while (!context.ExitFlag)
			{
				lock (context)
					Console.Write("\n> ");
				string cmd = Console.ReadLine().Trim();
				lock (context)
				{
					if (cmd.Length == 0) continue;
					BaseCommand? command = TryGetCommand(cmd);
					if (command == null)
						Console.WriteLine("No such command");
					else Console.WriteLine(command.Execute(context, cmd));
				}
			}
		}

		private static BaseCommand[] InitCommandList()
		{
			return new BaseCommand[] {
				new Commands.HelpCommand(),
				new Commands.ExitCommand(),
				new Commands.NewApiHandleCommand(),
				new Commands.DeleteApiHandleCommand(),
				new Commands.DeleteAllApiHandlesCommand(),
				new Commands.InfoApiHandleCommand(),
				new Commands.InfoApiHandleFactoryCommand(),
				new Commands.GetWeatherCommand(),
			};
		}

		private static BaseCommand? TryGetCommand(string cmd)
		{
			foreach (BaseCommand command in context.CommandList)
				if (command.IsMatch(cmd))
					return command;
			return null;
		}

		public static ApiHandle.ExceptionHandler MakePollingLoopExceptionHandler(string apiHandlerName)
		{
			return ex => {
				lock (context)
				{
					Console.WriteLine();
					Console.WriteLine("An error occured within the polling loop of '{0}' ApiHandler object:",
						apiHandlerName);
					Console.WriteLine(ex.Message);
					Console.WriteLine();
				}
			};
		}
	}
}
