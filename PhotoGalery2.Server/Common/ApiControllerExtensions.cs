using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace PhotoGalery2.Server
{
    public static class ApiControllerExtensions
    {
        public static HttpResponseMessage ContentStreamResult(this ApiController controller,
            Stream stream, string mimeType)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            return result;
        }

        public static void ThrowHttpErrorResponseException(this ApiController controller,
            HttpStatusCode code, string desc = null)
        {
            throw new HttpResponseException(new HttpResponseMessage(code)
            {
                ReasonPhrase = desc,
            });
        }
    }
}