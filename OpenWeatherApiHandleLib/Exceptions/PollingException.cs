using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	[Serializable]
	public class PollingException : Exception
	{
		public string CityName { get; }

		public PollingException(string cityName, Exception innerException) :
			base($"Exception while polling weather data for the '{cityName}' city", innerException)
		{
			CityName = cityName ?? throw new ArgumentNullException(nameof(cityName));
		}

		public PollingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			CityName = info.GetString(nameof(CityName)) ?? throw new NullReferenceException();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(CityName), CityName);
		}
	}
}
