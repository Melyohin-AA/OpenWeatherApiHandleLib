﻿using System;
using System.Runtime.Serialization;

namespace OpenWeatherApiHandleLib.Exceptions
{
	/// <summary>
	/// Represents an error that occurs
	/// if the API cannot find any location with specified city name.
	/// </summary>
	[Serializable]
	public class LocationNotFoundException : Exception
	{
		public string CityName { get; }

		public LocationNotFoundException(string cityName) : base($"'{cityName}' city is not found!")
		{
			CityName = cityName ?? throw new ArgumentNullException(nameof(cityName));
		}

		public LocationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
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
