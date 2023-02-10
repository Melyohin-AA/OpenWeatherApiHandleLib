using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	[Serializable]
	public class ApiKeyOccupiedException : Exception
	{
		public ApiKeyOccupiedException() : base() { }
		public ApiKeyOccupiedException(string? message) : base(message) { }
		public ApiKeyOccupiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public ApiKeyOccupiedException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
