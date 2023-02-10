using System;
using System.Net;
using System.Text;

namespace OpenWeatherApiHandleLib.Testing
{
	internal class FakeHttpMessageHandler : HttpMessageHandler
	{
		private readonly ResponseFunc responseFunc;

		public FakeHttpMessageHandler(ResponseFunc responseFunc)
		{
			this.responseFunc = responseFunc ?? throw new ArgumentNullException(nameof(responseFunc));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			string? responseStr = responseFunc(request);
			bool ok = responseStr != null;
			HttpContent? content = ok ? new FakeHttpContent(responseStr) : null;
			var response = new HttpResponseMessage {
				StatusCode = ok ? HttpStatusCode.OK : HttpStatusCode.NotFound,
				Content = content,
				RequestMessage = request,
				Version = new Version(1, 1),
			};
			return response;
		}

		public delegate string? ResponseFunc(HttpRequestMessage request);

		private class FakeHttpContent : HttpContent
		{
			private readonly string data;

			public FakeHttpContent(string data)
			{
				this.data = data ?? throw new ArgumentNullException(nameof(data));
			}

			protected override bool TryComputeLength(out long length)
			{
				length = data.Length;
				return true;
			}

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
				=> stream.WriteAsync(Encoding.UTF8.GetBytes(data)).AsTask();

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context,
				CancellationToken cancellationToken)
				=> stream.WriteAsync(Encoding.UTF8.GetBytes(data), cancellationToken).AsTask();

			protected override void SerializeToStream(Stream stream, TransportContext? context,
				CancellationToken cancellationToken)
				=> stream.Write(Encoding.UTF8.GetBytes(data));

			protected override Task<Stream> CreateContentReadStreamAsync()
				=> Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(data)));

			protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
				=> Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(data))).WaitAsync(cancellationToken);

			protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
				=> new MemoryStream(Encoding.UTF8.GetBytes(data));
		}
	}
}
