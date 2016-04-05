using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    [DataContract]
    public class AuthenticationResponse
    {
        [DataMember]
        public string AuthToken { get; set; }

        [DataMember]
        public string AuthType { get; set; }
    }
}