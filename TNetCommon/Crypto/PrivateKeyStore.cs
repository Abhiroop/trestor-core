using Chaos.NaCl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Crypto
{

    public class SinglePrivateAccount
    {
        public byte[] PublicKey;
        public string Name;//:"testingsoni",
        public byte[] Hash;//:"[B@42285098",
        public int EncryptionVersion;//:1,
        public string PasswordHint;//":"",
        public string Type;//":"Trestor Encrypted Key",
        public string Address;//":"TNeavqWy1ZjWfXqvbtsww1WjUHi3jGUkzpY",
        public byte[] CipherText;
        public byte[] Salt;//":"[B@41c21268"
    }

    
    public class PrivateKeyStore
    {
        int Kdf_Iterations = 4096;
        
        /*
         {
          "Type":"Trestor Encrypted Key",
          "EncryptionVersion":1,
          "Name":"hatshil",
          "Address":"TNUirtFv6ZqdoYzV8oy4eSUe3ucmG9GtVAH",
          "PublicKey":"86BF18F91DC7E0FC6837030CEDC3DB054B549BD44855672B167CC14068021285",
          "CipherText":"85E8275D579E9050FB050C3FD778F894E008A8D7D3F8F34D628C71EDE9F9F9464994675C06810A3A2C908A58DEAE9882",
          "Salt":"A19C514BA05166ECAAEC91A988B9062E",
          "Hash":"CEE0A2E7152EA9BCDBBF542BDEE9D13DF1DC4ADB",
          "PasswordHint":""
         }
        */

        private byte[] CryptoStreamProcess(ICryptoTransform transform, byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        string _test = "{\"Type\":\"Trestor Encrypted Key\",\"EncryptionVersion\":1,\"Name\":\"hatshil\",\"Address\":\"TNUirtFv6ZqdoYzV8oy4eSUe3ucmG9GtVAH\",\"PublicKey\":\"86BF18F91DC7E0FC6837030CEDC3DB054B549BD44855672B167CC14068021285\",\"CipherText\":\"85E8275D579E9050FB050C3FD778F894E008A8D7D3F8F34D628C71EDE9F9F9464994675C06810A3A2C908A58DEAE9882\",\"Salt\":\"A19C514BA05166ECAAEC91A988B9062E\",\"Hash\":\"CEE0A2E7152EA9BCDBBF542BDEE9D13DF1DC4ADB\",\"PasswordHint\":\"\"}";

        public PrivateKeyStore()
        {
            AddToStore(_test, "password");
        }
        
        public bool AddToStore(string SingleAccountJson, string password)
        {
            SinglePrivateAccount spa = 
                JsonConvert.DeserializeObject<SinglePrivateAccount>(SingleAccountJson, Common.JsonSerializerSettings);

            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(password, spa.Salt, Kdf_Iterations);

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = k1.GetBytes(32);
                aesAlg.IV = new byte[16];

                // Create a decrytor to perform the stream transform.             

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] PrivateKey = CryptoStreamProcess(decryptor, spa.CipherText);

                    MemoryStream ms = new MemoryStream();

                    ms.Write(Common.Encoding28591.GetBytes(password), 0, password.Length);
                    ms.Write(spa.Salt, 0, spa.Salt.Length);
                    ms.Write(PrivateKey, 0 ,PrivateKey.Length);

                    byte[] Hash = (new SHA1Managed()).ComputeHash(ms.ToArray());

                    byte[] PublicKey;
                    byte[] SecretKeyExpanded;
                    Ed25519.KeyPairFromSeed(out PublicKey, out SecretKeyExpanded, PrivateKey);
                }
            }

            return true;
        }

    }
}
