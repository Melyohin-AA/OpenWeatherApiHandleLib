using System;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface
{
	internal abstract class BaseCommand
	{
		protected const string apiHandleNotFoundMessage = "No ApiHandle with the specified name";

		public abstract string Description { get; }
		public abstract string Tip { get; }

		public abstract bool IsMatch(string cmd);

		public abstract string Execute(Context context, string cmd);
	}
}
