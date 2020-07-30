using NLog;
using TestProject.Utilities.Cryptography.CryptoInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Utilities.Cryptography.CryptoLogics
{
    /// <summary>
    /// Encryption and decryption using TripleDes.
    /// </summary>
    internal class TripleDesCrypto : ITestProjectCryptoProvider
    {
        private readonly ILogger _logger;
        private readonly string key = "p615m@t61p13dscc6ypt0@m3";

        public TripleDesCrypto(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// To encrypt string data
        /// </summary>
        /// <param name="queryString">The string which is to be encrypted</param>
        /// <returns>encrypted string</returns>
        public string EncryptString(string queryString)
        {
            string output = default(string);

            try
            {
                using (TripleDESCryptoServiceProvider tDes = new TripleDESCryptoServiceProvider())
                {
                    tDes.Key = Encoding.UTF8.GetBytes(key);
                    tDes.Mode = CipherMode.ECB;
                    tDes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform cTransform = tDes.CreateEncryptor())
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(queryString);
                        byte[] resultArray = cTransform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                        output = Convert.ToBase64String(resultArray, 0, resultArray.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return output;
        }

        /// <summary>
        /// To do string decryption.
        /// </summary>
        /// <param name="cryptoData">Encrypted data which is to be decrypted</param>
        /// <returns>Decrypted string</returns>
        public string DecryptString(string cryptoData)
        {
            string output = default(string);

            try
            {
                using (TripleDESCryptoServiceProvider tDes = new TripleDESCryptoServiceProvider())
                {
                    tDes.Key = Encoding.UTF8.GetBytes(key);
                    tDes.Mode = CipherMode.ECB;
                    tDes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform cTransform = tDes.CreateDecryptor())
                    {
                        byte[] inputBytes = Convert.FromBase64String(cryptoData);
                        byte[] resultArray = cTransform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                        output = Encoding.UTF8.GetString(resultArray);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return output;
        }
    }
}
