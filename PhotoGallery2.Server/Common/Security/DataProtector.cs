using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace PhotoGalery2.Server.Common.Security
{
    public class DataProtector
    {
        public const int CHECKSUM_LEN = 2;

        public byte[] Key { get; private set; }
        public byte[] IV { get; private set; }

        public DataProtector(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }

        public byte[] Protect(byte[] data)
        {
            byte[] checksum = ChecksumHelper.Checksum(data, CHECKSUM_LEN);
            byte[] dataWithChecksum = new byte[data.Length + checksum.Length];

            data.CopyTo(dataWithChecksum, 0);
            checksum.CopyTo(dataWithChecksum, data.Length);

            byte[] encryptedData = EncryptionHelper.EncryptAES(dataWithChecksum, Key, IV);

            return encryptedData;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            try
            {
                byte[] decryptedDataWithChecksum = EncryptionHelper.DecryptAES(protectedData, Key, IV);
                int n = decryptedDataWithChecksum.Length;

                byte[] rawData = new byte[n - CHECKSUM_LEN];
                byte[] checksum = new byte[CHECKSUM_LEN];

                Array.Copy(decryptedDataWithChecksum, rawData, rawData.Length);
                Array.Copy(decryptedDataWithChecksum, rawData.Length, checksum, 0, CHECKSUM_LEN);

                // recalculate checksum
                if (!checksum.SequenceEqual(ChecksumHelper.Checksum(rawData, CHECKSUM_LEN)))
                {
                    throw new CryptographicException("Checksum was invalid");
                }

                return rawData;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(string.Format("Unable to unprotect string: {0}", ex.Message), ex);
            }
        }
    }
}