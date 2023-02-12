using System;

namespace OpenWeatherApiHandleLib.ConsoleLineInterface
{
	internal class Context
	{
		public bool ExitFlag { get; set; }
		public BaseCommand[] CommandList { get; }
		public ApiHandleFactory ApiHandleFactory { get; }
		public Dictionary<string, ApiHandle> ApiHandles { get; }
		public Dictionary<string, ApiHandle.ExceptionHandler> PollingLoopExceptionHandlers { get; }

		public Context(BaseCommand[] commandList)
		{
			CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
			ApiHandleFactory = new ApiHandleFactory();
			ApiHandles = new Dictionary<string, ApiHandle>();
			PollingLoopExceptionHandlers = new Dictionary<string, ApiHandle.ExceptionHandler>();
		}
	}
}
