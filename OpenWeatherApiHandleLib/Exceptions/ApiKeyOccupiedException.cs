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
		public string ApiKey { get; }

		public ApiKeyOccupiedException(string apiKey) : base($"ApiKey '{apiKey}' is already in use!")
		{
			ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
		}

		public ApiKeyOccupiedException(SerializationInfo info, StreamingContext context) : base(info, context)
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
