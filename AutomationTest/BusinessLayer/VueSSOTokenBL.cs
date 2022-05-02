using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Philips.EDI.Foundation.APIGateway.AutomationTest.BusinessLayer
{
    public class VueSSOTokenBL
    {
        public string GenerateSsoToken(string username, string sessionTime, string symmetricKey)
        {
            var ssoToken = $"user_name={username}&session_time={sessionTime}";
            return EncryptSsoToken(ssoToken, symmetricKey);
        }

        private static string EncryptSsoToken(string toBeEncryptedUrl, string symmetricKey)
        {
            byte[] encrypted;
            var Key = Convert.FromBase64String(symmetricKey);
            byte[] IV = new byte[16];
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(toBeEncryptedUrl);
                        encrypted = ms.ToArray();
                        return HttpUtility.UrlEncode(Convert.ToBase64String(encrypted));
                    }
                }
            }
        }
    }
}
