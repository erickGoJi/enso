using System;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Enso.api.Utility
{
    public class Protection
    {
        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/47800a60-4461-4f8e-a8d1-751fa62c7884/aes-encrypt-in-javascript-and-decrypt-in-c?forum=csharpgeneral       
        //KEY MUST HAVE 16 LENGHT SIZE
        public static byte[] Encrypt(string json, IConfiguration configuration)
        {
            var keybytes = Encoding.UTF8.GetBytes("1P13n50110525SEC");
            var iv = Encoding.UTF8.GetBytes("1P13n50110525SEC");
            var encryptStringToBytes = EncryptStringToBytes(json, keybytes, iv);
            return encryptStringToBytes;
        }

        public static string DecryptBack(byte[]encrypted, IConfiguration configuration)
        {
            var keybytes = Encoding.UTF8.GetBytes("1P13n50110525SEC");
            var iv = Encoding.UTF8.GetBytes("1P13n50110525SEC");            
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return decriptedFromJavascript;
        }
        
        public static string Decrypt(string jsonString, IConfiguration configuration)
        {
            var keybytes = Encoding.UTF8.GetBytes("1P13n50110525SEC");
            var iv = Encoding.UTF8.GetBytes("1P13n50110525SEC");            
            var encrypted = Convert.FromBase64String(jsonString);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return decriptedFromJavascript;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0) {
                throw new ArgumentNullException("cipherText");
            } if (key == null || key.Length <= 0) {
                throw new ArgumentNullException("key");
            } if (iv == null || iv.Length <= 0) {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0) {
                throw new ArgumentNullException("plainText");
            } if (key == null || key.Length <= 0) {
                throw new ArgumentNullException("key");
            } if (iv == null || iv.Length <= 0) {
                throw new ArgumentNullException("key");
            }

            byte[] encrypted;
            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        //public static string Encrypt(string json, IConfiguration configuration)
        //{
        //    string PRIVATE_KEY = configuration.GetValue<string>("AppSettings:Secret");

        //    byte[] results;
        //    System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

        //    MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
        //    byte[] tdeskey = hashProvider.ComputeHash(UTF8.GetBytes(PRIVATE_KEY));
        //    TripleDESCryptoServiceProvider tripDES = new TripleDESCryptoServiceProvider();

        //    tripDES.Key = tdeskey;
        //    tripDES.Mode = CipherMode.CBC;
        //    tripDES.IV = new byte[tripDES.BlockSize / 8];
        //    tripDES.Padding = PaddingMode.PKCS7;            

        //    byte[] data = null;
        //    ICryptoTransform enc = null;

        //    enc = tripDES.CreateEncryptor();
        //    data = UTF8.GetBytes(json);

        //    try {
        //        results = enc.TransformFinalBlock(data, 0, data.Length);
        //    }
        //    finally {
        //        tripDES.Clear();
        //        hashProvider.Clear();
        //    }

        //    return Convert.ToBase64String(results);
        //}

        //public static string Decrypt(string json, IConfiguration configuration)
        //{
        //    string PRIVATE_KEY = configuration.GetValue<string>("AppSettings:Secret");

        //    byte[] results;
        //    System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

        //    MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
        //    byte[] tdeskey = hashProvider.ComputeHash(UTF8.GetBytes(PRIVATE_KEY));
        //    TripleDESCryptoServiceProvider tripDES = new TripleDESCryptoServiceProvider();

        //    tripDES.Key = tdeskey;
        //    tripDES.Mode = CipherMode.CBC;
        //    tripDES.IV = new byte[tripDES.BlockSize / 8];
        //    tripDES.Padding = PaddingMode.PKCS7;

        //    byte[] data = null;
        //    ICryptoTransform enc = null;

        //    enc = tripDES.CreateDecryptor();
        //    data = Convert.FromBase64String(json);

        //    try
        //    {
        //        results = enc.TransformFinalBlock(data, 0, data.Length);
        //    }
        //    finally
        //    {
        //        tripDES.Clear();
        //        hashProvider.Clear();
        //    }

        //    return UTF8.GetString(results);
        //}
    }
}
