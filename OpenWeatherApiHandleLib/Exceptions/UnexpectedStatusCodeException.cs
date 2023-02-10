using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	[Serializable]
	public class UnexpectedStatusCodeException : Exception
	{
		public UnexpectedStatusCodeException() : base() { }
		public UnexpectedStatusCodeException(string? message) : base(message) { }
		public UnexpectedStatusCodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public UnexpectedStatusCodeException(string? message, Exception? innerException) :
			base(message, innerException) { }

		public UnexpectedStatusCodeException(System.Net.HttpStatusCode statusCode, string request) :
			base($"Unexpected '{statusCode}' status code for '{request}' request!") { }
	}
}
