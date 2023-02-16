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
		public string ApiKey { get; }

		public InvalidApiKeyException(string apiKey) : base($"The API considers '{apiKey}' key as invalid one!")
		{
			ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
		}

		public InvalidApiKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ApiKey = info.GetString(nameof(ApiKey)) ?? throw new NullReferenceException();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(ApiKey), ApiKey);
		}
	}
}
