//
// WebRequestHelper.cs
//
// Author:
//       Bojan Rajkovic <bojan.rajkovic@xamarin.com>
//       Michael Hutchinson <mhutch@xamarin.com>
//
// based on NuGet src/Core/Http
//
// Copyright (c) 2013-2014 Xamarin Inc.
// Copyright (c) 2010-2014 Outercurve Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mono.Addins.Setup
{
	/// <summary>
	/// Helper for making web requests with support for authenticated proxies.
	/// </summary>
	public static class WebRequestHelper
	{
		static Func<Func<HttpWebRequest>, Action<HttpWebRequest>,CancellationToken,HttpWebResponse> _handler;

		/// <summary>
		/// Sets a custom request handler that can handle requests for authenticated proxy servers.
		/// </summary>
		/// <param name="handler">The custom request handler.</param>
		public static void SetRequestHandler (Func<Func<HttpWebRequest>, Action<HttpWebRequest>,CancellationToken,HttpWebResponse> handler)
		{
			_handler = handler;
		}

		/// <summary>
		/// Gets the web response, using the request handler to handle proxy authentication
		/// if necessary.
		/// </summary>
		/// <returns>The response.</returns>
		/// <param name="createRequest">Callback for creating the request.</param>
		/// <param name="prepareRequest">Callback for preparing the request, e.g. writing the request stream.</param>
		/// <param name="token">Cancellation token.</param>
		/// <remarks>
		/// Keeps sending requests until a response code that doesn't require authentication happens or if the request
		/// requires authentication and the user has stopped trying to enter them (i.e. they hit cancel when they are prompted).
		/// </remarks>
		public static Task<HttpWebResponse> GetResponseAsync (
			Func<HttpWebRequest> createRequest,
			Action<HttpWebRequest> prepareRequest = null,
			CancellationToken token = default(CancellationToken))
		{
			return Task.Factory.StartNew (() => GetResponse (createRequest, prepareRequest, token), token);
		}

		/// <summary>
		/// Gets the web response, using the request handler to handle proxy authentication
		/// if necessary.
		/// </summary>
		/// <returns>The response.</returns>
		/// <param name="createRequest">Callback for creating the request.</param>
		/// <param name="prepareRequest">Callback for preparing the request, e.g. writing the request stream.</param>
		/// <param name="token">Cancellation token.</param>
		/// <remarks>
		/// Keeps sending requests until a response code that doesn't require authentication happens or if the request
		/// requires authentication and the user has stopped trying to enter them (i.e. they hit cancel when they are prompted).
		/// </remarks>
		public static HttpWebResponse GetResponse (
			Func<HttpWebRequest> createRequest,
			Action<HttpWebRequest> prepareRequest = null,
			CancellationToken token = default(CancellationToken))
		{
			var handler = _handler;
			if (handler != null)
				return handler (createRequest, prepareRequest, token);

			var req = createRequest ();
			if (token.CanBeCanceled)
				token.Register (req.Abort);
			if (prepareRequest != null)
				prepareRequest (req);
			return (HttpWebResponse) req.GetResponse ();
		}

		/// <summary>
		/// Determines whether an error code is likely to have been caused by internet reachability problems.
		/// </summary>
		public static bool IsCannotReachInternetError (this WebExceptionStatus status)
		{
			switch (status) {
			case WebExceptionStatus.NameResolutionFailure:
			case WebExceptionStatus.ConnectFailure:
			case WebExceptionStatus.ConnectionClosed:
			case WebExceptionStatus.ProxyNameResolutionFailure:
			case WebExceptionStatus.SendFailure:
			case WebExceptionStatus.Timeout:
				return true;
			default:
				return false;
			}
		}
	}
}
