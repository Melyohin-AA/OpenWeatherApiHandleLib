using System;
using System.Net;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	/// <summary>
	/// Represents an error caused by receiving a response from the API with an unexpected status code.
	/// </summary>
	[Serializable]
	public class UnexpectedStatusCodeException : Exception
	{
		public HttpStatusCode StatusCode { get; }
		public string Request { get; }

		public UnexpectedStatusCodeException(HttpStatusCode statusCode, string request) :
			base($"Unexpected '{statusCode}' status code for '{request}' request!")
		{
			StatusCode = statusCode;
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}

		public UnexpectedStatusCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			StatusCode = (HttpStatusCode)info.GetUInt16(nameof(StatusCode));
			Request = info.GetString(nameof(Request)) ?? throw new NullReferenceException();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(StatusCode), (ushort)StatusCode);
			info.AddValue(nameof(Request), Request);
		}
	}
}
