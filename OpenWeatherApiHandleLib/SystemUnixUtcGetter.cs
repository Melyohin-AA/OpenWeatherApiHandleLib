using System;

namespace OpenWeatherApiHandleLib
{
	public sealed class SystemUnixUtcGetter : IUnixUtcGetter
	{
		public long Seconds => ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
	}
}
