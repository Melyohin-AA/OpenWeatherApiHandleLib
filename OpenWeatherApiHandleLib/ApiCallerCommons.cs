using System;

namespace OpenWeatherApiHandleLib
{
	/// <summary>
	/// Commons of ApiCaller classes.
	/// </summary>
	internal static class ApiCallerCommons
	{
		/// <summary>
		/// Builds HTTP request URI with sceme, authority, path and parameters.
		/// </summary>
		/// <param name="address">Part of URL which consists of sceme, authority, path</param>
		/// <param name="parameters"></param>
		/// <returns>Built HTTP request URI</returns>
		public static string BuildRequestUri(string address, Dictionary<string, string> parameters)
		{
			System.Collections.Specialized.NameValueCollection pb =
				System.Web.HttpUtility.ParseQueryString(string.Empty);
			foreach (var pair in parameters)
				pb.Add(pair.Key, pair.Value);
			return $"{address}?{pb}";
		}

		/// <summary>
		/// Makes HTTP request with the specified URI through the specified 'HttpClient' object.
		/// Does initial processing of the response.
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="httpClient"></param>
		/// <param name="apiKey"></param>
		/// <returns>The text response</returns>
		/// <exception cref="Exceptions.InvalidApiKeyException"></exception>
		/// <exception cref="Exceptions.UnexpectedStatusCodeException"></exception>
		public static string GetResponse(string requestUri, HttpClient httpClient, string apiKey)
		{
			Task<HttpResponseMessage> task = httpClient.GetAsync(requestUri);
			task.Wait();
			switch (task.Result.StatusCode)
			{
				case System.Net.HttpStatusCode.OK:
					break;
				case System.Net.HttpStatusCode.Unauthorized:
					throw new Exceptions.InvalidApiKeyException($"The API considers '{apiKey}' key as invalid one!");
				default:
					throw new Exceptions.UnexpectedStatusCodeException(task.Result.StatusCode, requestUri);
			}
			var responseReadTask = task.Result.Content.ReadAsStringAsync();
			responseReadTask.Wait();
			return responseReadTask.Result;
		}
	}
}
