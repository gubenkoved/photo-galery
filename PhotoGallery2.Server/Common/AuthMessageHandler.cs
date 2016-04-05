﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public class AuthMessageHandler : DelegatingHandler
    {
        public const string AuthenticationType = "EGToken";
        public const string QueryStringAuthHolderKey = "$auth";

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null)
            {
                if (string.Equals(request.Headers.Authorization.Scheme, AuthenticationType, StringComparison.InvariantCultureIgnoreCase))
                {
                    string authToken = request.Headers.Authorization.Parameter;

                    // validate Token here
                    HandleAuthData(request, authToken);
                }
            } else // try query string parameters to find token
            {
                var authTokenFound = request.GetQueryNameValuePairs()
                    .Any(k => k.Key == QueryStringAuthHolderKey);

                if (authTokenFound)
                {
                    var tokenKvp = request.GetQueryNameValuePairs()
                        .First(k => k.Key == QueryStringAuthHolderKey);

                    HandleAuthData(request, tokenKvp.Value);
                }
            }

            var innerResult = await base.SendAsync(request, cancellationToken);

            return innerResult;
        }

        private void HandleAuthData(HttpRequestMessage request, string token)
        {
            var identity = new ClaimsIdentity(AuthenticationType);

            var principal = new ClaimsPrincipal(identity);

            request.GetRequestContext().Principal = principal;
            //SetPrincipal(principal);
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
        }
    }
}