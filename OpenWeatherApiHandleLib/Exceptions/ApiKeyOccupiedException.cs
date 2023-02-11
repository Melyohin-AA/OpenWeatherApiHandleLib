using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	/// <summary>
	/// Represents an error caused by an attempt to create 'ApiHandle' object with an ApiKey
	/// which is already in use in another 'ApiHandle' object.
	/// </summary>
	[Serializable]
	public class ApiKeyOccupiedException : Exception
	{
		public ApiKeyOccupiedException() : base() { }
		public ApiKeyOccupiedException(string? message) : base(message) { }
		public ApiKeyOccupiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public ApiKeyOccupiedException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
