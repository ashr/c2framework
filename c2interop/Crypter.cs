using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace c2interop{
    //This is total security by obscurity, but at least it will require disassembly to be able to decrypt (If you only deploy the binaries)
    //Should obfuscate as well
    public class Crypter{
        private const string ivc = "uOfOkQT4S6jnoyM2Kyx/f4McFtXlSMeKksIrSk7NKJkuQzCQJt+bfvHLP6StwVdA2nq5bfrmMwGqXceZopFDFubHFsPqt3qdvVk01om4As4=";
        private const string keyc = "zXh5XzyLUBBhUb+rxlWy9aMxQ7ZHF+5TP+2AP/bVezuVO7x55ZfMWm3C3GGOnE1tMBJVnDjKfnAnK93WxDgfsQt40WFYVNJgNm3BMs9ybb4=";
        private string initVector = "INITIAL VECTOR FOR ENCRYPTION";
        private string initKey = "ENCODING KEY FOR ENCRYPTION";
        private const string zacSpeed = "79d901e2a7cb25fe7e6da54b0b21883f682ba453462265598b7ec3ea0ee1e5e8";

        public byte[] Decode (string Encoded){
            return Convert.FromBase64String(Encoded);
        }
        public string Decrypt(string Encrypted){
            return decrypt(Encrypted,keyc);
        }

        public string Encrypt(string ClearText){
            return encrypt(ClearText,zacSpeed);
        }

       #region cryptography
        private string decrypt(string ciphertext, string key)
        {
            if (string.IsNullOrEmpty(ciphertext))
            {
                throw new ArgumentNullException("ciphertext");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            initKey = decryptInternal(key,zacSpeed);

            var allTheBytes = Convert.FromBase64String(ciphertext);
            var saltBytes = allTheBytes.Take(32).ToArray();
            var ciphertextBytes = allTheBytes.Skip(32).Take(allTheBytes.Length - 32).ToArray();

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(initKey, saltBytes))
            {
                var keyBytes = keyDerivationFunction.GetBytes(32);
                var ivBytes = keyDerivationFunction.GetBytes(16);

                return DecryptWithAES(ciphertextBytes, keyBytes, ivBytes);
            }
        }

        private string decryptInternal(string ciphertext, string key)
        {
            if (string.IsNullOrEmpty(ciphertext))
            {
                throw new ArgumentNullException("ciphertext");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            var allTheBytes = Convert.FromBase64String(ciphertext);
            var saltBytes = allTheBytes.Take(32).ToArray();
            var ciphertextBytes = allTheBytes.Skip(32).Take(allTheBytes.Length - 32).ToArray();

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(key, saltBytes))
            {
                var keyBytes = keyDerivationFunction.GetBytes(32);
                var ivBytes = keyDerivationFunction.GetBytes(16);

                return DecryptWithAES(ciphertextBytes, keyBytes, ivBytes);
            }
        }        

        private string encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException("plainText");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            initKey = decryptInternal(keyc,zacSpeed);

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(initKey, 32))
            {
                byte[] saltBytes = keyDerivationFunction.Salt;
                byte[] keyBytes = keyDerivationFunction.GetBytes(32);
                byte[] ivBytes = keyDerivationFunction.GetBytes(16);

                using (var aesManaged = new AesManaged())
                {
                    aesManaged.KeySize = 256;

                    using (var encryptor = aesManaged.CreateEncryptor(keyBytes, ivBytes))
                    {
                        MemoryStream memoryStream = null;
                        CryptoStream cryptoStream = null;

                        return WriteMemoryStream(plainText, ref saltBytes, encryptor, ref memoryStream, ref cryptoStream);
                    }
                }
            }
        }

        private string WriteMemoryStream(string plainText, ref byte[] saltBytes, ICryptoTransform encryptor, ref MemoryStream memoryStream, ref CryptoStream cryptoStream)
        {
            try
            {
                memoryStream = new MemoryStream();

                try
                {
                    cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                    using (var streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                }
                finally
                {
                    if (cryptoStream != null)
                    {
                        cryptoStream.Dispose();
                    }
                }

                var cipherTextBytes = memoryStream.ToArray();
                Array.Resize(ref saltBytes, saltBytes.Length + cipherTextBytes.Length);
                Array.Copy(cipherTextBytes, 0, saltBytes, 32, cipherTextBytes.Length);

                return Convert.ToBase64String(saltBytes);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
            }
        }

        private string DecryptWithAES(byte[] ciphertextBytes, byte[] keyBytes, byte[] ivBytes)
        {
            using (var aesManaged = new AesManaged())
            {
                using (var decryptor = aesManaged.CreateDecryptor(keyBytes, ivBytes))
                {
                    MemoryStream memoryStream = null;
                    CryptoStream cryptoStream = null;
                    StreamReader streamReader = null;

                    try
                    {
                        memoryStream = new MemoryStream(ciphertextBytes);
                        cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                        streamReader = new StreamReader(cryptoStream);

                        return streamReader.ReadToEnd();
                    }
                    finally
                    {
                        if (memoryStream != null)
                        {
                            memoryStream.Dispose();
                            memoryStream = null;
                        }
                    }
                }
            }
        }
        #endregion        
    }
}