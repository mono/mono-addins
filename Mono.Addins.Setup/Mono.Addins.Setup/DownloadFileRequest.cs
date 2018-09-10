//
// WebRequestWrapper.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mono.Addins.Setup
{
	abstract class DownloadFileRequest : IDisposable
	{
		public abstract void Dispose ();
		public abstract int ContentLength { get; }
		public abstract Stream Stream { get; }

		public static Task<DownloadFileRequest> DownloadFile (string url, bool noCache)
		{
			if (HttpClientProvider.HasCustomCreation || !WebRequestHelper.HasCustomRequestHandler)
				return HttpClientDownloadFileRequest.Create (url, noCache);

			return WebRequestDownloadFileRequest.Create (url, noCache);
		}
	}

	class HttpClientDownloadFileRequest : DownloadFileRequest
	{
		HttpClient client;
		HttpResponseMessage response;
		Stream stream;

		public static Task<DownloadFileRequest> Create (string url, bool noCache)
		{
			// Use Task.Run to avoid hanging the UI thread when waiting for the GetAsync method to return
			// with the response for an .mpack file download.
			return Task.Run<DownloadFileRequest> (async () => {
				var client = HttpClientProvider.CreateHttpClient (url);
				if (noCache)
					client.DefaultRequestHeaders.Add ("Pragma", "no-cache");

				var response = await client.GetAsync (url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait (false);
				var stream = await response.Content.ReadAsStreamAsync ().ConfigureAwait (false);

				return new HttpClientDownloadFileRequest {
					client = client,
					response = response,
					stream = stream
				};
			});
		}

		public override int ContentLength {
			get { return (int)response.Content.Headers.ContentLength; }
		}

		public override Stream Stream {
			get { return stream; }
		}

		public override void Dispose ()
		{
			stream?.Dispose ();
			response?.Dispose ();
			client.Dispose ();
		}
	}

	class WebRequestDownloadFileRequest : DownloadFileRequest
	{
		WebResponse response;
		Stream stream;

		public static Task<DownloadFileRequest> Create (string url, bool noCache)
		{
			var response = WebRequestHelper.GetResponse (
				() => (HttpWebRequest)WebRequest.Create (url),
				r => {
					if (noCache)
						r.Headers ["Pragma"] = "no-cache";
				}
			);

			var request = new WebRequestDownloadFileRequest {
				response = response,
				stream = response.GetResponseStream ()
			};

			return Task.FromResult<DownloadFileRequest> (request);
		}

		public override int ContentLength {
			get { return (int)response.ContentLength; }
		}

		public override Stream Stream {
			get { return stream; }
		}

		public override void Dispose ()
		{
			stream?.Dispose ();
			response.Dispose ();
		}
	}
}
