using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public static class EncodingHelper
    {
        #region Constants
        private const string BASE64_STRING_DELIMITER = ".";
        #endregion

        #region Hex
        public static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new FormatException("Hex strings must have even len");
            }

            return Enumerable.Range(0, hexString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => System.Convert.ToByte(hexString.Substring(x, 2), 16))
                .ToArray();
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        #endregion

        #region Base64
        public static string Base64Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Base64Decode(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        public static string Base64StringsConcat(params string[] base64Strings)
        {
            foreach (var s in base64Strings)
            {
                if (!IsBase64String(s))
                {
                    throw new ArgumentException(string.Format("{0} is not base 64 string", s));
                }
            }

            return string.Join(BASE64_STRING_DELIMITER, base64Strings);
        }

        public static string[] SplitConcatenatedBase64Strings(string text, int expectedSegmentAmount)
        {
            string[] result = text.Split(new[] { BASE64_STRING_DELIMITER }, StringSplitOptions.None);

            if (result.Length != expectedSegmentAmount)
            {
                throw new FormatException(string.Format("{0} segments delimited with '{0}' has been expected"));
            }

            return result;
        }
        #endregion

        #region URL Encoding
        #region Base64 custom URL encoding

        // these two methods intended to avoid necessity of Base64 string URL encoding to bypass
        // complexities of handling the encoding on Angular SPA side (angular does not like the % signs)
        // after this custom encoding valid base64 string it becomes string that is valid URL component and does not need
        // any URL encoding/decoding to be applied

        public static string Base64CustomUrlEncode(string base64stringRaw)
        {
            return base64stringRaw
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace('=', '~');
        }

        public static string Base64CustomUrlDecode(string base64stringEncoded)
        {
            return base64stringEncoded
                .Replace('-', '+')
                .Replace('_', '/')
                .Replace('~', '=');
        }
        #endregion

        public static string URLEncode(string text)
        {
            return System.Web.HttpUtility.UrlEncode(text);
        }

        public static string URLDecode(string text)
        {
            return System.Web.HttpUtility.UrlDecode(text);
        }

        #endregion

        #region Helpers
        private static bool IsBase64String(this string s)
        {
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
        #endregion
    }
}