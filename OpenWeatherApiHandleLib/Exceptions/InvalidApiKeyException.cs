using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	/// <summary>
	/// Represents an error that occurs
	/// when the API's server responses with '401' status code
	/// which means that used ApiKey is invalid.
	/// </summary>
	[Serializable]
	public class InvalidApiKeyException : Exception
	{
		public InvalidApiKeyException() : base() { }
		public InvalidApiKeyException(string? message) : base(message) { }
		public InvalidApiKeyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public InvalidApiKeyException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
