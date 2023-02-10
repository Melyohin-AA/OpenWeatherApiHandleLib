using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	[Serializable]
	public class LocationNotFoundException : Exception
	{
		public LocationNotFoundException() : base() { }
		public LocationNotFoundException(string? message) : base(message) { }
		public LocationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public LocationNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
