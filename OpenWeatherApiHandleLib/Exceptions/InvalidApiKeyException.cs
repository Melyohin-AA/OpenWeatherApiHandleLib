using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	[Serializable]
	public class InvalidApiKeyException : Exception
	{
		public InvalidApiKeyException() : base() { }
		public InvalidApiKeyException(string? message) : base(message) { }
		public InvalidApiKeyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public InvalidApiKeyException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
