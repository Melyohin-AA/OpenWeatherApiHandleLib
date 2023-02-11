using System;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Defines a source of current UNIX-style UTC.
	/// </summary>
	public interface IUnixUtcGetter
	{
		/// <summary>
		/// The UNIX-style UTC in seconds.
		/// </summary>
		long Seconds { get; }
	}
}
