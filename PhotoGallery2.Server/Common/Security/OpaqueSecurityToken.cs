using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace PhotoGalery2.Server.Common.Security
{
    /// <summary>
    /// Container for encrypted and hard to guess/tamper piece of data.
    /// Presence of anonymous security key gives ability to have no additional credentials to access limited set of API methods.
    /// </summary>
    public class OpaqueSecurityToken
    {
        /// <summary>
        /// Contains known security key payload keys.
        /// Modifying this values could broke the links that contains affected keys, because
        /// it will make data looks like missing (it will be located under different key as the result of change).
        /// 
        /// IMPORTANT: Do NOT change these values if you do not completely understand consequences.
        /// </summary>
        public static class KnownPayloadKeys
        {
            public const string GENERATION_DATE = "gen.date";
            public const string USERNAME = "username";
            public const string TTL_SEC = "ttl.sec";
        }

        #region Data stored inside key
        /// <summary>
        /// Gets or sets initialization vector for AES encryption.
        /// </summary>
        public byte[] IV { get; private set; }

        /// <summary>
        /// Unified secure storage for data - will be part of security key.
        /// </summary>
        public Dictionary<string, string> SecurePayload { get; private set; }

        /// <summary>
        /// Gets UTC data indicating when this link was generated.
        /// </summary>
        public DateTime GenerationDateUTC
        {
            get { return DateTime.Parse(SecurePayload[KnownPayloadKeys.GENERATION_DATE]); }
            private set { SecurePayload[KnownPayloadKeys.GENERATION_DATE] = value.ToString(); }
        }
        #endregion

        #region Constructors
        internal OpaqueSecurityToken()
            : this(EncryptionHelper.GenerateIV())
        {
        }

        internal OpaqueSecurityToken(byte[] iv)
        {
            SecurePayload = new Dictionary<string, string>();

            IV = iv;
            GenerationDateUTC = DateTime.UtcNow;
        }

        internal OpaqueSecurityToken(byte[] iv, Dictionary<string, string> payload)
        {
            IV = iv;
            SecurePayload = payload;
        }
        #endregion

        #region Serialization
        public string SerializeToString()
        {
            return SerializeToString(Config.Instance.EncryptionKey);
        }

        public string SerializeToString(byte[] aesKey)
        {
            string payloadString = SerializePayload(SecurePayload);

            byte[] payloadData = Encoding.UTF8.GetBytes(payloadString);

            DataProtector dataProtector = new DataProtector(aesKey, IV);

            byte[] encryptedPayloadData = dataProtector.Protect(payloadData);

            string base64encryptedPayload = EncodingHelper.Base64Encode(encryptedPayloadData);

            string base64iv = EncodingHelper.Base64Encode(IV);

            return EncodingHelper.Base64CustomUrlEncode(EncodingHelper.Base64StringsConcat(base64iv, base64encryptedPayload));
        }

        public static OpaqueSecurityToken Parse(string serializedToken)
        {
            return Parse(Config.Instance.EncryptionKey, serializedToken);
        }

        public static OpaqueSecurityToken Parse(byte[] aesKey, string serializedToken)
        {
            try
            {
                serializedToken = EncodingHelper.Base64CustomUrlDecode(serializedToken);

                string[] splittedIVandPayload = EncodingHelper.SplitConcatenatedBase64Strings(serializedToken, 2);

                byte[] iv = EncodingHelper.Base64Decode(splittedIVandPayload[0]);

                DataProtector dataProtector = new DataProtector(aesKey, iv);

                byte[] payloadProtected = EncodingHelper.Base64Decode(splittedIVandPayload[1]);

                byte[] payloadData = dataProtector.Unprotect(payloadProtected);

                string payloadString = Encoding.UTF8.GetString(payloadData);

                Dictionary<string, string> payload = DeserializePayload(payloadString);

                return new OpaqueSecurityToken(iv, payload);
            }
            catch (Exception ex)
            {
                throw new SecurityException(string.Format("Unable to parse security key: '{0}'", serializedToken), ex);
            }
        }

        #region Helpers
        private static string SerializePayload(Dictionary<string, string> payload)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new JavaScriptSerializer();

                string serialized = serializer.Serialize(payload);

                return serialized;
            }
        }

        private static Dictionary<string, string> DeserializePayload(string serializedPayload)
        {
            var serializer = new JavaScriptSerializer();

            var result = serializer.Deserialize<Dictionary<string, string>>(serializedPayload);

            return result;
        }
        #endregion
        #endregion

        #region Methods
        public static OpaqueSecurityToken Create()
        {
            return new OpaqueSecurityToken();
        }
        #endregion
    }
}