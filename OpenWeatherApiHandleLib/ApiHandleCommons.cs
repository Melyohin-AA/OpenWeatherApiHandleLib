using System;
using System.Globalization;

namespace OpenWeatherApiHandleLib
{
	internal static class ApiHandleCommons
	{
		public static string BuildRequest(string url, Dictionary<string, string> parameters)
		{
			System.Collections.Specialized.NameValueCollection pb =
				System.Web.HttpUtility.ParseQueryString(string.Empty);
			foreach (var pair in parameters)
				pb.Add(pair.Key, pair.Value);
			return $"{url}?{pb}";
		}

		public static string GetResponse(string request, HttpClient httpClient, string apiKey)
		{
			Task<HttpResponseMessage> task = httpClient.GetAsync(request);
			task.Wait();
			switch (task.Result.StatusCode)
			{
				case System.Net.HttpStatusCode.OK:
					break;
				case System.Net.HttpStatusCode.Unauthorized:
					throw new Exceptions.InvalidApiKeyException($"The API considers '{apiKey}' key as invalid one!");
				default:
					throw new Exceptions.UnexpectedStatusCodeException(task.Result.StatusCode, request);
			}
			var responseReadTask = task.Result.Content.ReadAsStringAsync();
			responseReadTask.Wait();
			return responseReadTask.Result;
		}
	}
}
