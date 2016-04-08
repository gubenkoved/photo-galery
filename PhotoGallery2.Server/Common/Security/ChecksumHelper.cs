using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace PhotoGalery2.Server.Common.Security
{
    public static class ChecksumHelper
    {
        public static byte[] Checksum(byte[] data, int lenBytes = 2)
        {
            if (lenBytes > 20)
            {
                throw new InvalidOperationException("20 bytes is maximal checksum len");
            }

            SHA1 sha = new SHA1CryptoServiceProvider();

            byte[] result = sha.ComputeHash(data);

            return result.Take(lenBytes).ToArray();
        }
    }
}