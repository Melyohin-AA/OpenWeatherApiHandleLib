using System;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Represents the mode of cached weather data updating for the 'ApiHandle' class.
	/// </summary>
	public enum ApiHandleUpdateMode
	{
		/// <summary>
		/// Cached weather data is updated on user requests if the data is not relevant.
		/// </summary>
		OnDemand,

		/// <summary>
		/// Cached weather data is updated within the polling loop.
		/// </summary>
		Polling,
	}
}
