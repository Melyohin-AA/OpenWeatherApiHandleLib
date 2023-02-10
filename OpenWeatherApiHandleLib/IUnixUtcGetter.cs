using System;

namespace OpenWeatherApiHandleLib
{
	public interface IUnixUtcGetter
	{
		long Seconds { get; }
	}
}
