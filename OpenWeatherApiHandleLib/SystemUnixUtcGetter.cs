using System;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Represents a source of current UNIX-style UTC based on 'System.DateTime'.
	/// </summary>
	public sealed class SystemUnixUtcGetter : IUnixUtcGetter
	{
		/// <summary>
		/// The UNIX-style UTC in seconds.
		/// </summary>
		public long Seconds => ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
	}
}
