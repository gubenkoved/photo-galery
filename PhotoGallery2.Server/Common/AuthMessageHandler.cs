using System;
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

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null)
            {
                if (string.Equals(request.Headers.Authorization.Scheme, AuthenticationType, StringComparison.InvariantCultureIgnoreCase))
                {
                    string authToken = request.Headers.Authorization.Parameter;

                    // validate Token here

                    var identity = new ClaimsIdentity(AuthenticationType);

                    var principal = new ClaimsPrincipal(identity);

                    request.GetRequestContext().Principal = principal;
                    //SetPrincipal(principal);
                }
            }

            var innerResult = await base.SendAsync(request, cancellationToken);

            return innerResult;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;
        }
    }
}