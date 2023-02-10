using System;

namespace OpenWeatherApiHandleLib.Testing
{
	internal class FakeUnixUtcGetter : IUnixUtcGetter
	{
		public long Seconds { get; set; }
	}
}
