using PhotoGalery2.Server.Common;
using PhotoGalery2.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PhotoGalery2.Server.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public AuthenticationResponse Authenticate(AuthenticationRequest authRequest)
        {
            if (authRequest.Username == "admin" && authRequest.Password == "admin")
            {
                return new AuthenticationResponse()
                {
                    AuthToken = DateTime.UtcNow.ToString(),
                    AuthType = AuthMessageHandler.AuthenticationType,
                };
            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        //[AllowAnonymous]
        [HttpGet]
        [Route("whoami")]
        public IIdentity WhoAmI()
        {
            if (Thread.CurrentPrincipal.Identity != null)
            {
                return Thread.CurrentPrincipal.Identity;
            }

            return null;
        }
    }
}
