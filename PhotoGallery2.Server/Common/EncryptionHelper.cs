using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace PhotoGalery2.Server.Common.Security
{
    public class EncryptionHelper
    {
        #region Fields
        private static Random _random = new Random(Guid.NewGuid().GetHashCode());

        public const int DEFAULT_BLOCK_SIZE_BITS = 128;
        #endregion

        /// <summary>
        /// Generates random initialization vector to be used in encryption/decryption processes.
        /// <param name="len">Len of IV in bytes.</param>
        /// </summary>
        public static byte[] GenerateIV(int len = DEFAULT_BLOCK_SIZE_BITS / 8)
        {
            return GetRandomBytesThreadSafe(len);
        }

        public static byte[] EncryptAES(byte[] data, byte[] key, byte[] iv)
        {
            #region Check arguments
            if (data == null || data.Length <= 0)
            {
                throw new ArgumentException("data");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentException("key");
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentException("iv");
            }
            #endregion

            byte[] encrypted;

            using (AesManaged aesAlg = CreateAES(key, iv))
            {
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (BinaryWriter bytesWriter = new BinaryWriter(csEncrypt))
                    {
                        bytesWriter.Write(data);
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }

            return encrypted;
        }

        public static byte[] DecryptAES(byte[] encrytedData, byte[] key, byte[] iv)
        {
            #region Arguments validation
            if (encrytedData == null || encrytedData.Length <= 0)
            {
                throw new ArgumentException("encrytedData");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentException("key");
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentException("iv");
            }
            #endregion

            byte[] data = null;

            // Create an AesManaged object 
            // with the specified key and IV. 
            using (AesManaged aesAlg = CreateAES(key, iv))
            {
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(encrytedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        data = ReadFully(csDecrypt);
                    }
                }

            }

            return data;
        }

        public static byte[] GetRandomBytesThreadSafe(int len)
        {
            lock (_random)
            {
                byte[] bytes = new byte[len];

                _random.NextBytes(bytes);

                return bytes;
            }
        }

        #region Helpers
        private static AesManaged CreateAES(byte[] key, byte[] iv)
        {
            return new AesManaged()
            {
                Key = key,
                IV = iv,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
            };
        }

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        #endregion
    }
}