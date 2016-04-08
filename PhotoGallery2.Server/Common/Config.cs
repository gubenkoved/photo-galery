using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public class Config
    {
        public static Config Instance = new Config();


        public byte[] EncryptionKey { get; set; }

        public Config()
        {
            // https://www.random.org/cgi-bin/randbyte?nbytes=16&format=h
            EncryptionKey = new byte[]
            {
                0xf4, 0xdb, 0x99, 0xa3, 0xe0, 0xe7, 0xdb, 0x6c, 0xc9, 0x23, 0x58, 0xfa, 0xc0, 0x94, 0xd6, 0xf1
            };
        }
    }
}